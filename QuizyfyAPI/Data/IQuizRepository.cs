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
        Task<Quiz> GetQuizAsync(string name, bool includeTalks = false);       
    }
}

