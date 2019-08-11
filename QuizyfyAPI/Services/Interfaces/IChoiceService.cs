using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Domain;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface IChoiceService : IService
    {
        Task<ObjectResult<ChoiceResponse[]>> GetAll(int quizId, int questionId);
        Task<ObjectResult<ChoiceResponse>> Get(int quizId, int questionId, int choiceId);
        Task<ObjectResult<ChoiceResponse>> Create(int quizId, int questionId, ChoiceCreateRequest request);
        Task<ObjectResult<ChoiceResponse>> Update(int quizId, int questionId, int choiceId, ChoiceUpdateRequest request);
        Task<DetailedResult> Delete(int quizId, int questionId, int choiceId);
    }
}
