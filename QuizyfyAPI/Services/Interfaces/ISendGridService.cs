using QuizyfyAPI.Data;
using SendGrid;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface ISendGridService
    {
        Task<Response> SendEmailTo(User user);
    }
}
