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
using reCAPTCHA.AspNetCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly PwnedPasswordsClient _pwnedPasswordsClient;
        private readonly IMapper _mapper;

        public UserService(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IOptions<JwtOptions> jwtOptions, TokenValidationParameters tokenValidationParameters, PwnedPasswordsClient pwnedPasswordsClient, IMapper mapper)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtOptions = jwtOptions.Value;
            _tokenValidationParameters = tokenValidationParameters;
            _pwnedPasswordsClient = pwnedPasswordsClient;
            _mapper = mapper;
        }

        public async Task<ObjectResult<UserResponse>> Login(UserLoginRequest request)
        {
            var user = await _userRepository.Authenticate(request.Username, request.Password.Normalize(NormalizationForm.FormKC));

            if (user != null)
            {
                return new ObjectResult<UserResponse> { Success = true, Object = _mapper.Map<UserResponse>(await RequestToken(user)) };
            }
            return new ObjectResult<UserResponse> { Success = false, Errors = new[] { "Wrong user credentials" } };
        }
        public async Task<BasicResult> Register(UserRegisterRequest request)
        {
            var user = _mapper.Map<User>(request);

            if (await _userRepository.GetUserByUsername(request.Username) != null)
            {
                return new BasicResult { Errors = new[] { "Username: " + user.Username + " is already taken" } };
            }

            if (await _pwnedPasswordsClient.HasPasswordBeenPwned(request.Password))
            {
                return new BasicResult { Errors = new[] { "This password has been leaked in data leak. Please use different password." } };
            }

            PasswordHash.Create(request.Password.Normalize(NormalizationForm.FormKC), out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _userRepository.Add(user);

            await _userRepository.SaveChangesAsync();

            return new BasicResult { Success = true };
        }
        public async Task<ObjectResult<UserResponse>> Update(int userId, UserUpdateRequest request)
        {
            var user = await _userRepository.GetUserById(userId);

            if (user == null)
            {
                return new ObjectResult<UserResponse> { Errors = new[] { $"Couldn't find user with id of {userId}" } };
            }
            else if (user.Username != request.Username && await _userRepository.GetUserByUsername(request.Username) != null)
            {
                return new ObjectResult<UserResponse> { Errors = new[] { "User with this username already exists!" } };
            }

            _userRepository.Update(user);

            if (!string.IsNullOrEmpty(request.Password))
            {
                PasswordHash.Create(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _mapper.Map(request, user);

            if(await _userRepository.SaveChangesAsync())
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

            if(await _userRepository.SaveChangesAsync())
            {
                return new DetailedResult { Success = true, Found = true };
            }

            return new DetailedResult {Found = true, Errors = new[] { "Action didn't affect any rows" } };
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

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;
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
}
