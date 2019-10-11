using QuizyfyAPI.Data;
using SendGrid;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface ISendGridService
    {
        Task<Response> SendConfirmationEmailTo(User user);
        Task<Response> SendPasswordResetEmailTo(User user);
    }
}
