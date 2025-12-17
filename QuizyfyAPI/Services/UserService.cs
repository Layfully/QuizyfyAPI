using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PwnedPasswords.Client;
using QuizyfyAPI.Helpers;
using QuizyfyAPI.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Mappers;
using QuizyfyAPI.Services.Interfaces;
using SendGrid;

namespace QuizyfyAPI.Services;

internal sealed partial class UserService : IUserService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IPwnedPasswordsClient _pwnedPasswordsClient;
    private readonly ISendGridService _mailService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRefreshTokenRepository refreshTokenRepository, 
        IUserRepository userRepository, 
        IOptions<JwtOptions> jwtOptions, 
        TokenValidationParameters tokenValidationParameters, 
        IPwnedPasswordsClient pwnedPasswordsClient, 
        ISendGridService mailService,
        TimeProvider timeProvider,
        ILogger<UserService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
        _tokenValidationParameters = tokenValidationParameters.Clone();
        _pwnedPasswordsClient = pwnedPasswordsClient;
        _mailService = mailService;
        _timeProvider = timeProvider;
        _logger = logger;
    }
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "Token validation failed")]
    private static partial void LogTokenValidationFailed(ILogger logger, Exception ex);

    public async Task<ObjectResult<UserResponse>> Login(UserLoginRequest request)
    {
        User? user = await _userRepository.Authenticate(request.Username, request.Password.Normalize(NormalizationForm.FormKC));

        if (user is null)
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
            Object = userWithToken.ToResponse()
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

        User user = request.ToEntity();
        
        Hash.Create(request.Password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.VerificationToken = Guid.NewGuid().ToString();

        _userRepository.Add(user);

        if(!await _userRepository.SaveChangesAsync())
        {
            return new BasicResult { Errors = ["User registration failed."] };
        }

        Response sendConfirmationResponse = await _mailService.SendConfirmationEmailTo(user);

        if (sendConfirmationResponse.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.OK)
        {
            return new BasicResult { Success = true };
        }
        
        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
        string body = await sendConfirmationResponse.Body.ReadAsStringAsync();
        return new BasicResult { Errors = [$"Sending registration email failed. {body}"] };
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

        user.UpdateFrom(request);
        
        if (!string.IsNullOrEmpty(request.Password))
        {
            Hash.Create(request.Password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
        }
        
        _userRepository.Update(user);

        if (await _userRepository.SaveChangesAsync())
        {
            return new ObjectResult<UserResponse> { Success = true, Object = user.ToResponse() };
        }

        return new ObjectResult<UserResponse> { Errors = ["No rows were affected"] };
    }

    public async Task<DetailedResult> Delete(int userId)
    {
        User? user = await _userRepository.GetUserById(userId);

        if (user is null)
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

    [SuppressMessage("Security", "CA5404:Do not disable token validation checks")]
    public async Task<ObjectResult<UserResponse>> RefreshTokenAsync(UserRefreshRequest request)
    {
        TokenValidationParameters? tokenParams = _tokenValidationParameters.Clone();
        tokenParams.ValidateLifetime = false; 
        
        ClaimsPrincipal? validatedToken = GetPrincipalFromToken(request.JwtToken, tokenParams);

        if (validatedToken is null)
        {
            return new ObjectResult<UserResponse> { Errors = ["Invalid token"] };
        }

        long expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value, CultureInfo.InvariantCulture);
        DateTime expiryDateUtc = DateTime.UnixEpoch.AddSeconds(expiryDateUnix);
        
        if (expiryDateUtc > _timeProvider.GetUtcNow().DateTime)
        {
            return new ObjectResult<UserResponse> { Errors = ["This token hasn't expired yet"] };
        }

        string jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

        RefreshToken? storedRefreshToken = await _refreshTokenRepository.GetRefreshToken(request.RefreshToken);

        if (storedRefreshToken is null)
        {
            return new ObjectResult<UserResponse> { Errors = ["This refresh token doesn't exist"] };
        }

        if (_timeProvider.GetUtcNow().DateTime > storedRefreshToken.ExpiryDate)
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
        User? user = await _userRepository.GetUserById(int.Parse(userIdClaim, CultureInfo.InvariantCulture));

        if (user is null)
        {
            return new ObjectResult<UserResponse> { Errors = ["User not found"] };
        }
        
        user = await RequestToken(user);

        return new ObjectResult<UserResponse>
        {
            Success = true,
            Object = user.ToResponse()
        };
    }

    public async Task<User> RequestToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
        DateTime now = _timeProvider.GetUtcNow().DateTime;
        
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString(CultureInfo.InvariantCulture))
            ]),

            Expires = now.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);

        user.RefreshToken = new RefreshToken
        {
            UserId = user.Id,
            JwtId = token.Id,
            CreationDate = now,
            ExpiryDate = now.AddMonths(1)
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
            return new ObjectResult<UserResponse> { Success = true, Object = user.ToResponse() };
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
            return new ObjectResult<UserResponse> { Success = true, Object = user.ToResponse() };
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
            return new ObjectResult<UserResponse> { Success = true, Object = user.ToResponse() };
        }

        return new ObjectResult<UserResponse> { Errors = ["No rows were affected"] };
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private ClaimsPrincipal? GetPrincipalFromToken(string token, TokenValidationParameters validationParameters)
    {
        JwtSecurityTokenHandler tokenHandler = new();

        try
        {       
            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken? validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            LogTokenValidationFailed(_logger, ex);
            return null;
        }
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return validatedToken is JwtSecurityToken jwtSecurityToken &&
               jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                   StringComparison.OrdinalIgnoreCase);
    }
}
