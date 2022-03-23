using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Domain;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services;
public interface IQuestionService : IService
{
    Task<ObjectResult<QuestionResponse[]>> GetAll(int quizId, bool includeChoices);
    Task<ObjectResult<QuestionResponse>> Get(int quizId, int questionId, bool includeChoices = false);
    Task<ObjectResult<QuestionResponse>> Create(int quizId, QuestionCreateRequest request);
    Task<ObjectResult<QuestionResponse>> Update(int quizId, int questionId, QuestionUpdateRequest request);
    Task<DetailedResult> Delete(int quizId, int questionId);
}
