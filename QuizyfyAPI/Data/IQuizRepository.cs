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
        Task<Quiz[]> GetAllQuizzesAsync(bool includeTalks = false);
        Task<Quiz> GetQuizAsync(int id, bool includeTalks = false);
        Task<Question[]> GetQuestionsByIdAsync(int id, bool includeChoices = false);
        Task<Question> GetQuestionByIdAsync(int quizId, int id, bool includeChoices = false);
        Task<Choice[]> GetChoicesForQuestion(int questionId);
        Task<User> GetUserById(int userId);
        Task<User> GetUserByUsername(string username);
        Task<User> Authenticate(string username, string password);
    }
}

