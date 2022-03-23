using System.Threading.Tasks;

namespace QuizyfyAPI.Data;
public interface IQuestionRepository : IRepository
{
    Task<Question[]> GetQuestions(int quizId, bool includeChoices = false);
    Task<Question> GetQuestion (int quizId, int questionId, bool includeChoices = false);
}

