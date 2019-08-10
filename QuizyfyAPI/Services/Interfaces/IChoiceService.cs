using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface IChoiceService : IService
    {
        Task<ObjectResult<ChoiceModel[]>> GetAll(int quizId, int questionId);
        Task<ObjectResult<ChoiceModel>> Get(int quizId, int questionId, int choiceId);
        Task<ObjectResult<ChoiceModel>> Create(int quizId, int questionId, ChoiceModel model);
        Task<ObjectResult<ChoiceModel>> Update(int quizId, int questionId, int choiceId, ChoiceModel model);
        Task<DetailedResult> Delete(int quizId, int questionId, int choiceId);
    }
}
