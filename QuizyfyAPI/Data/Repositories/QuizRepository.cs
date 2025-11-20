using Microsoft.EntityFrameworkCore;
using QuizyfyAPI.Contracts.Responses.Pagination;

namespace QuizyfyAPI.Data;
public class QuizRepository : Repository, IQuizRepository
{
    public QuizRepository(QuizDbContext context, ILogger<QuizRepository> logger) : base(context, logger)
    {
    }

    // Changed return type to Task<PagedList> and marked async
    public async Task<PagedList<Quiz>> GetQuizzes(PagingParams pagingParams, bool includeQuestions = false)
    {
        _logger.LogInformation("Getting all quizzes");

        IQueryable<Quiz> query = _context.Quizzes.AsNoTracking();

        if (includeQuestions)
        {
            _logger.LogInformation("Including questions in query");
            query = query.Include(quiz => quiz.Questions)
                .ThenInclude(question => question.Choices);
        }

        query = query.Include(quiz => quiz.Image);

        return await PagedList<Quiz>.CreateAsync(query, pagingParams.PageNumber, pagingParams.PageSize);
    }

    public Task<Quiz?> GetQuiz(int id, bool includeQuestions = false)
    {
        _logger.LogInformation("Getting quiz with id {Id}", id);

        IQueryable<Quiz> query = _context.Quizzes;

        if (includeQuestions)
        {
            _logger.LogInformation("Including questions in query");
            query = query.Include(quiz => quiz.Questions)
                .ThenInclude(question => question.Choices);
        }

        return query.FirstOrDefaultAsync(quiz => quiz.Id == id);
    }
}