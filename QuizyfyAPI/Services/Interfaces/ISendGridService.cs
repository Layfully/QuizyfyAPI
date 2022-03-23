using QuizyfyAPI.Data;
using SendGrid;

namespace QuizyfyAPI.Services;
public interface ISendGridService
{
    Task<Response> SendConfirmationEmailTo(User user);
    Task<Response> SendPasswordResetEmailTo(User user);
}
