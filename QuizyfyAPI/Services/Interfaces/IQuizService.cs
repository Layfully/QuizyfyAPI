using Microsoft.AspNetCore.Http;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface IQuizService : IService
    {
        Task<ObjectResult<QuizListModel>> GetAll(PagingParams pagingParams, HttpResponse response, HttpContext httpContext);
        Task<ObjectResult<QuizModel>> Get(int id, bool includeQuestions);
        Task<ObjectResult<QuizModel>> Create(QuizCreateModel model);
        Task<ObjectResult<QuizModel>> Update(int quizId, QuizCreateModel model);
        Task<DetailedResult> Delete(int quizId);
    }
}
