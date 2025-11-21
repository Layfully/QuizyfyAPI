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
using SendGrid;

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
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRefreshTokenRepository refreshTokenRepository, 
        IUserRepository userRepository, 
        IOptions<JwtOptions> jwtOptions, 
        TokenValidationParameters tokenValidationParameters, 
        IPwnedPasswordsClient pwnedPasswordsClient, 
        IMapper mapper, 
        ISendGridService mailService,
        ILogger<UserService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
        _tokenValidationParameters = tokenValidationParameters.Clone();
        _pwnedPasswordsClient = pwnedPasswordsClient;
        _mapper = mapper;
        _mailService = mailService;
        _logger = logger;

        _tokenValidationParameters.ValidateLifetime = false;
    }

    public async Task<ObjectResult<UserResponse>> Login(UserLoginRequest request)
    {
        User? user = await _userRepository.Authenticate(request.Username, request.Password.Normalize(NormalizationForm.FormKC));

        if (user == null)
        {
            return new ObjectResult<UserResponse> { Success = false, Errors = ["Wrong user credentials"] };
            
        }
        
        if (!user.EmailConfirmed)
        {
            return new ObjectResult<UserResponse> { Success = false, Errors = ["Email is not verified"] };
        }
        
        User userWithToken = await RequestToken(user);
        
        return new ObjectResult<UserResponse> 
        { 
            Success = true, 
            Object = _mapper.Map<UserResponse>(userWithToken) 
        };
    }

    public async Task<BasicResult> Register(UserRegisterRequest request)
    {
        if (await _userRepository.GetUserByUsername(request.Username) is not null)
        {
            return new BasicResult { Errors = [$"Username: {request.Username} is already taken"] };
        }

        if (await _userRepository.GetUserByEmail(request.Email) is not null)
        {
            return new BasicResult { Errors = [$"Email: {request.Email} is already taken"] };
        }

        if (await _pwnedPasswordsClient.HasPasswordBeenPwned(request.Password))
        {
            return new BasicResult { Errors = ["This password has been leaked in a data breach. Please use a different password."] };
        }

        User? user = _mapper.Map<User>(request);
        
        Hash.Create(request.Password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.VerificationToken = Guid.NewGuid().ToString();

        _userRepository.Add(user);

        if(!await _userRepository.SaveChangesAsync())
        {
            return new BasicResult { Errors = new[] { "User registration failed." } };
        }

        Response sendConfirmationResponse = await _mailService.SendConfirmationEmailTo(user);

        if (sendConfirmationResponse.StatusCode != HttpStatusCode.Accepted && sendConfirmationResponse.StatusCode != HttpStatusCode.OK)
        {
            _userRepository.Delete(user);
            await _userRepository.SaveChangesAsync();
            string body = await sendConfirmationResponse.Body.ReadAsStringAsync();
            return new BasicResult { Errors = [$"Sending registration email failed. {body}"] };
        }

        return new BasicResult { Success = true };
    }

    public async Task<ObjectResult<UserResponse>> Update(int userId, UserUpdateRequest request)
    {
        User? user = await _userRepository.GetUserById(userId);

        if (user is null)
        {
            return new ObjectResult<UserResponse> { Errors = [$"Couldn't find user with id of {userId}"] };
        }

        if (user.Username != request.Username && await _userRepository.GetUserByUsername(request.Username!) is not null)
        {
            return new ObjectResult<UserResponse> { Errors = ["User with this username already exists!"] };
        }

        if (user.Email != request.Email && await _userRepository.GetUserByEmail(request.Email!) is not null)
        {
            return new ObjectResult<UserResponse> { Errors = ["User with this email already exists!"] };
        }

        _mapper.Map(request, user);
        
        if (!string.IsNullOrEmpty(request.Password))
        {
            Hash.Create(request.Password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
        }
        
        _userRepository.Update(user);

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = ["No rows were affected"] };
    }

    public async Task<DetailedResult> Delete(int userId)
    {
        User? user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return new DetailedResult { Errors = ["User with given id was not found"] };
        }

        _userRepository.Delete(user);

        if (await _userRepository.SaveChangesAsync())
        {
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<ObjectResult<UserResponse>> RefreshTokenAsync(UserRefreshRequest request)
    {
        ClaimsPrincipal? validatedToken = GetPrincipalFromToken(request.JwtToken);

        if (validatedToken is null)
        {
            return new ObjectResult<UserResponse> { Errors = ["Invalid token"] };
        }

        long expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
        DateTime expiryDateUtc = DateTime.UnixEpoch.AddSeconds(expiryDateUnix);
        
        if (expiryDateUtc > DateTime.UtcNow)
        {
            return new ObjectResult<UserResponse> { Errors = ["This token hasn't expired yet"] };
        }

        string jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

        RefreshToken? storedRefreshToken = await _refreshTokenRepository.GetRefreshToken(request.RefreshToken);

        if (storedRefreshToken is null)
        {
            return new ObjectResult<UserResponse> { Errors = ["This refresh token doesn't exist"] };
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return new ObjectResult<UserResponse> { Errors = ["Token Expired"] };
        }

        if (storedRefreshToken.Invalidated)
        {
            return new ObjectResult<UserResponse> { Errors = ["This refresh token has been invalidated"] };
        }

        if (storedRefreshToken.Used)
        {
            return new ObjectResult<UserResponse> { Errors = ["This refresh token has been used"] };
        }

        if (storedRefreshToken.JwtId != jti)
        {
            return new ObjectResult<UserResponse> { Errors = ["This refresh token does not match this JWT"] };
        }

        storedRefreshToken.Used = true;
        _refreshTokenRepository.UpdateRefreshToken(storedRefreshToken);

        await _userRepository.SaveChangesAsync();

        string userIdClaim = validatedToken.Claims.Single(x => x.Type == "id").Value;
        User user = await _userRepository.GetUserById(int.Parse(userIdClaim));

        if (user is null)
        {
            return new ObjectResult<UserResponse> { Errors = ["User not found"] };
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
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
        
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString())
            ]),

            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);

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
        User? user = await _userRepository.GetUserById(userId);

        if (user is null)
        {
            return new ObjectResult<UserResponse> { Errors = [$"Couldn't find user with id of {userId}"] };
        }

        if (user.VerificationToken != token)
        {
            return new ObjectResult<UserResponse> { Errors = ["Email verification token is invalid!"] };
        }
        
        user.EmailConfirmed = true;
        user.VerificationToken = null;

        _userRepository.Update(user);

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = ["No rows were affected"] };
    }

    public async Task<ObjectResult<UserResponse>> RecoverPassword(int userId, string token, string password)
    {
        User? user = await _userRepository.GetUserById(userId);

        if (user is null)
        {
            return new ObjectResult<UserResponse> { Errors = [$"Couldn't find user with id of {userId}"] };
        }

        if (user.RecoveryToken != token)
        {
            return new ObjectResult<UserResponse> { Errors = ["Password recovery token is invalid!"] };
        }

        if (!string.IsNullOrEmpty(password))
        {
            Hash.Create(password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.RecoveryToken = null;
        }
        
        _userRepository.Update(user);
        
        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = ["No rows were affected"] };
    }

    public async Task<ObjectResult<UserResponse>> GenerateRecoveryToken(RecoveryTokenGenerationRequest request)
    {
        User? user = await _userRepository.GetUserByEmail(request.Email);

        if (user is null)
        {
            return new ObjectResult<UserResponse> { Errors = [$"Couldn't find user with email: {request.Email}"] };
        }

        user.RecoveryToken = Guid.NewGuid().ToString();
        _userRepository.Update(user);
        
        Response sendConfirmationResponse = await _mailService.SendPasswordResetEmailTo(user);
        
        if (sendConfirmationResponse.StatusCode != HttpStatusCode.Accepted && sendConfirmationResponse.StatusCode != HttpStatusCode.OK)
        {
            return new ObjectResult<UserResponse> { Errors = ["Failed to send recovery email"] };
        }

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(user) };
        }

        return new ObjectResult<UserResponse> { Errors = ["No rows were affected"] };
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new();

        try
        {       
            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken? validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
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
