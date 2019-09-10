using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizyfyAPI.Contracts.Responses.Pagination;

namespace QuizyfyAPI.Data
{
    public class QuizRepository : Repository, IQuizRepository
    {

        public QuizRepository(QuizDbContext context, ILogger<QuizRepository> logger) : base(context, logger)
        {
        }

        public PagedList<Quiz> GetQuizzes(PagingParams pagingParams, bool includeQuestions = false)
        {
            _logger.LogInformation($"Getting all quizzes");

            IQueryable<Quiz> query = _context.Quizzes;

            if (includeQuestions)
            {
                _logger.LogInformation($"With questions");
                query = query.Include(quiz => quiz.Questions).ThenInclude(question => question.Choices);
            }

            query = query.Include(quiz => quiz.Image);
            PagedList<Quiz> pagedQuizzes = new PagedList<Quiz>(query, pagingParams.PageNumber, pagingParams.PageSize);

            return pagedQuizzes;
        }

        public async Task<Quiz> GetQuiz(int id, bool includeQuestions = false)
        {
            _logger.LogInformation($"Getting one quiz");

            IQueryable<Quiz> query = _context.Quizzes;

            if (includeQuestions)
            {
                _logger.LogInformation($"With questions");
                query = query.Include(quiz => quiz.Questions).ThenInclude(question => question.Choices);
            }

            query = query.Where(quiz => quiz.Id == id);

            return await query.FirstOrDefaultAsync();
        }
    }
}
