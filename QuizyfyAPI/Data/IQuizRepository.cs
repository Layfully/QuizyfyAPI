using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public interface IQuizRepository
    {

        // General 
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        // Camps
        Task<Quiz[]> GetQuizzes(bool includeTalks = false);
        Task<Quiz> GetQuiz(int id, bool includeTalks = false);
        Task<Question[]> GetQuestions(int quizId, bool includeChoices = false);
        Task<Question> GetQuestion (int quizId, int questionId, bool includeChoices = false); 
        Task<Choice[]> GetChoices(int quizId,int questionId);
        Task<Choice> GetChoice(int quizId, int questionId, int choiceId);
        Task<User> GetUserById(int userId);
        Task<User> GetUserByUsername(string username);
        Task<User> Authenticate(string username, string password);
    }
}

