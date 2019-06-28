using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public interface IChoiceRepository : IRepository
    {
        Task<Choice[]> GetChoices(int quizId,int questionId);
        Task<Choice> GetChoice(int quizId, int questionId, int choiceId);
    }
}

