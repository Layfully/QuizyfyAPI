using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuizyfyAPI.Models;

namespace QuizyfyAPI.Data
{
    public interface IQuestionRepository : IRepository
    {
        Task<Question[]> GetQuestions(int quizId, bool includeChoices = false);
        Task<Question> GetQuestion (int quizId, int questionId, bool includeChoices = false);
    }
}

