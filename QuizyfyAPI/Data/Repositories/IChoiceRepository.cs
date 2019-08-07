using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public interface IChoiceRepository : IRepository
    {
        Task<Choice[]> GetChoices(int questionId);
        Task<Choice> GetChoice(int questionId, int choiceId);
    }
}

