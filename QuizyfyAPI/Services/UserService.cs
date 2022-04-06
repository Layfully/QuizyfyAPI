using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PwnedPasswords.Client;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Helpers;
using QuizyfyAPI.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace QuizyfyAPI.Services;
public class UserService : IUserService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IPwnedPasswordsClient _pwnedPasswordsClient;
    private readonly IMapper _mapper;
    private readonly ISendGridService _mailService;

    public UserService(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IOptions<JwtOptions> jwtOptions, TokenValidationParameters tokenValidationParameters, IPwnedPasswordsClient pwnedPasswordsClient, IMapper mapper, ISendGridService mailService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
        _pwnedPasswordsClient = pwnedPasswordsClient;
        _mapper = mapper;
        _mailService = mailService;

        _tokenValidationParameters = tokenValidationParameters.Clone();
        _tokenValidationParameters.ValidateLifetime = false;
    }

    public async Task<ObjectResult<UserResponse>> Login(UserLoginRequest request)
    {
        var user = await _userRepository.Authenticate(request.Username, request.Password.Normalize(NormalizationForm.FormKC));

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Success = false, Errors = new[] { "Wrong user credentials" } };
        }
        else if (!user.EmailConfirmed)
        {
            return new ObjectResult<UserResponse> { Success = false, Errors = new[] { "Email is not verified" } };
        }
        return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(await RequestToken(user)) };
    }

    public async Task<BasicResult> Register(UserRegisterRequest request)
    {
        var user = _mapper.Map<User>(request);

        if (await _userRepository.GetUserByUsername(request.Username) != null)
        {
            return new BasicResult { Errors = new[] { "Username: " + user.Username + " is already taken" } };
        }

        if (await _userRepository.GetUserByEmail(request.Email) != null)
        {
            return new BasicResult { Errors = new[] { "Email: " + user.Email + " is already taken" } };
        }

        if (await _pwnedPasswordsClient.HasPasswordBeenPwned(request.Password))
        {
            return new BasicResult { Errors = new[] { "This password has been leaked in data leak. Please use different password." } };
        }

        Hash.Create(request.Password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.VerificationToken = Guid.NewGuid().ToString();

        _userRepository.Add(user);

        if(!await _userRepository.SaveChangesAsync())
        {
            return new BasicResult { Errors = new[] { "User registration failed." } };
        }

        var sendConfirmationResponse = await _mailService.SendConfirmationEmailTo(user);

        if (sendConfirmationResponse.StatusCode != HttpStatusCode.Accepted)
        {
            _userRepository.Delete(user);
            await _userRepository.SaveChangesAsync();
            return new BasicResult { Errors = new[] { "Sending registration email failed." + await sendConfirmationResponse.Body.ReadAsStringAsync() + " ----- Headers ------ " + sendConfirmationResponse.Headers.ToString()} };
        }

        return new BasicResult { Success = true };
    }

    public async Task<ObjectResult<UserResponse>> Update(int userId, UserUpdateRequest request)
    {
        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { $"Couldn't find user with id of {userId}" } };
        }

        if (user.Username != request.Username && await _userRepository.GetUserByUsername(request.Username) != null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "User with this username already exists!" } };
        }

        if (user.Email != request.Email && await _userRepository.GetUserByEmail(request.Email) != null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "User with this email already exists!" } };
        }

        _userRepository.Update(user);

        if (!string.IsNullOrEmpty(request.Password))
        {
            Hash.Create(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
        }

        _mapper.Map(request, user);

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = new[] { "No rows were affected" } };
    }

    public async Task<DetailedResult> Delete(int userId)
    {
        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return new DetailedResult { Errors = new[] { "User with given id was not found" } };
        }

        _userRepository.Delete(user);

        if (await _userRepository.SaveChangesAsync())
        {
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }

    public async Task<ObjectResult<UserResponse>> RefreshTokenAsync(UserRefreshRequest request)
    {
        var validatedToken = GetPrincipalFromToken(request.JwtToken);

        if (validatedToken == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "Invalid token" } };
        }

        var expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
        var expiryDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDateUnix);

        if (expiryDateUtc > DateTime.UtcNow)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "This token hasn't expired yet" } };
        }

        var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

        var storedRefreshToken = await _refreshTokenRepository.GetRefreshToken(request.RefreshToken);

        if (storedRefreshToken == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "This refresh token doesn't exist" } };
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "Token Expired" } };
        }

        if (storedRefreshToken.Invalidated)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "This refresh token has been invalidated" } };
        }

        if (storedRefreshToken.Used)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "This refresh token has been used" } };
        }

        if (storedRefreshToken.JwtId != jti)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "This refresh token does not match this JWT" } };
        }

        storedRefreshToken.Used = true;
        _refreshTokenRepository.UpdateRefreshToken(storedRefreshToken);

        await _userRepository.SaveChangesAsync();

        var user = await _userRepository.GetUserById(Int32.Parse(validatedToken.Claims.Single(x => x.Type == ClaimTypes.Name).Value));

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "This refresh token does not belong to any user" } };
        }

        user = await RequestToken(user);

        return new ObjectResult<UserResponse>
        {
            Success = true,
            Object = _mapper.Map<UserResponse>(user)
        };
    }

    public async Task<User> RequestToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", user.Id.ToString())
            }),

            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        user.RefreshToken = new RefreshToken()
        {
            UserId = user.Id,
            JwtId = token.Id,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(1)
        };
        user.JwtToken = tokenHandler.WriteToken(token);

        _refreshTokenRepository.Add(user.RefreshToken);
        await _userRepository.SaveChangesAsync();
        await _refreshTokenRepository.SaveChangesAsync();
        return user;
    }

    public async Task<ObjectResult<UserResponse>> VerifyEmail(int userId, string token)
    {
        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { $"Couldn't find user with id of {userId}" } };
        }

        if (user.VerificationToken != token)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "Email verification token is invalid!" } };
        }

        _userRepository.Update(user);

        user.EmailConfirmed = true;

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = new[] { "No rows were affected" } };
    }

    public async Task<ObjectResult<UserResponse>> RecoverPassword(int userId, string token, string password)
    {
        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { $"Couldn't find user with id of {userId}" } };
        }

        if (user.RecoveryToken != token)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { "Password recovery token is invalid!" } };
        }

        _userRepository.Update(user);

        if (!string.IsNullOrEmpty(password))
        {
            Hash.Create(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.RecoveryToken = null;
        }

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = new[] { "No rows were affected" } };
    }

    public async Task<ObjectResult<UserResponse>> GenerateRecoveryToken(RecoveryTokenGenerationRequest request)
    {

        var user = await _userRepository.GetUserByEmail(request.Email);

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Errors = new[] { $"Couldn't find user with email: {request.Email}" } };
        }

        _userRepository.Update(user);

        user.RecoveryToken = Guid.NewGuid().ToString();

        var sendConfirmationResponse = await _mailService.SendPasswordResetEmailTo(user);

        if (sendConfirmationResponse.StatusCode != HttpStatusCode.Accepted)
        {
            user.RecoveryToken = null;
        }

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = new[] { "No rows were affected" } };
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {       
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
               jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                   StringComparison.InvariantCultureIgnoreCase);
    }
}
