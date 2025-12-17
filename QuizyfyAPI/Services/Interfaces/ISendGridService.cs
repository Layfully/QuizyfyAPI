using QuizyfyAPI.Data.Entities;
using SendGrid;

namespace QuizyfyAPI.Services.Interfaces;

internal interface ISendGridService
{
    Task<Response> SendConfirmationEmailTo(User user);
    Task<Response> SendPasswordResetEmailTo(User user);
}
