using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface IQuestionService
    {
        Task<ObjectResult<QuestionModel[]>> GetAll(int quizId, bool includeChoices);
        Task<ObjectResult<QuestionModel>> Get(int quizId, int questionId, bool includeChoices);
        Task<ObjectResult<QuestionModel>> Create(int quizId, QuestionCreateModel model);
        Task<ObjectResult<QuestionModel>> Update(int quizId, int questionId, QuestionCreateModel model);
        Task<DetailedResult> Delete(int quizId, int questionId);
    }
}
