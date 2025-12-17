using QuizyfyAPI.Contracts.Responses.Pagination;
using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Data.Repositories.Interfaces;

internal interface IQuizRepository : IRepository
{
    Task<PagedList<Quiz>> GetQuizzes(PagingParams pagingParams, bool includeQuestions = false);
    
    Task<Quiz?> GetQuiz(int id, bool includeQuestions = false);
}