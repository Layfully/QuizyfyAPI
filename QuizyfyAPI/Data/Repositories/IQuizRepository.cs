using QuizyfyAPI.Contracts.Responses.Pagination;

namespace QuizyfyAPI.Data;
public interface IQuizRepository : IRepository
{
    PagedList<Quiz> GetQuizzes(PagingParams pagingParams, bool includeQuestions = false);
    Task<Quiz?> GetQuiz(int id, bool includeQuestions = false);
}

