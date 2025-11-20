using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Contracts.Responses.Pagination;
using QuizyfyAPI.Domain;

namespace QuizyfyAPI.Services;
public interface IQuizService : IService
{
    Task<ObjectResult<QuizListResponse>> GetAll(PagingParams pagingParams, HttpContext httpContext);
    Task<ObjectResult<QuizResponse>> Get(int id, bool includeQuestions);
    Task<ObjectResult<QuizResponse>> Create(QuizCreateRequest request);
    Task<ObjectResult<QuizResponse>> Update(int quizId, QuizUpdateRequest request);
    Task<DetailedResult> Delete(int quizId);
}
