using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Domain;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services;
public interface ILikeService
{
    Task<ObjectResult<LikeResponse>> Like(int quizId, int userId);

    Task<DetailedResult> Delete(int quizId, int userId);
}
