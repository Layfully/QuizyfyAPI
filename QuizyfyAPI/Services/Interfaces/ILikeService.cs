using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface ILikeService
    {
        Task<ObjectResult<LikeModel>> Like(int quizId, int userId);

        Task<DetailedResult> Delete(int quizId, int userId);
    }
}
