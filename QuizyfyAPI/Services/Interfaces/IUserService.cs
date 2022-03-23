using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services;
public interface IUserService : IService
{
    Task<ObjectResult<UserResponse>> Login(UserLoginRequest request);
    Task<BasicResult> Register(UserRegisterRequest request);
    Task<ObjectResult<UserResponse>> Update(int userId, UserUpdateRequest request);
    Task<DetailedResult> Delete(int userId);
    Task<ObjectResult<UserResponse>> VerifyEmail(int userId, string token);
    Task<ObjectResult<UserResponse>> RecoverPassword(int userId, string token, string password);
    Task<ObjectResult<UserResponse>> GenerateRecoveryToken(RecoveryTokenGenerationRequest request);
    Task<ObjectResult<UserResponse>> RefreshTokenAsync(UserRefreshRequest request);
    Task<User> RequestToken(User user);
}
