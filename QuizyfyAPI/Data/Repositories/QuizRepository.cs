using QuizyfyAPI.Contracts.Responses.Pagination;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal sealed partial class QuizRepository(QuizDbContext context, ILogger<QuizRepository> logger) : Repository(context, logger), IQuizRepository
{
    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting quizzes. Page: {PageNumber}, Size: {PageSize}, IncludeQuestions: {IncludeQuestions}")]
    private static partial void LogGettingQuizzes(ILogger logger, int pageNumber, int pageSize, bool includeQuestions);

    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting quiz {Id}. IncludeQuestions: {IncludeQuestions}")]
    private static partial void LogGettingQuiz(ILogger logger, int id, bool includeQuestions);
    
    public async Task<PagedList<Quiz>> GetQuizzes(PagingParams pagingParams, bool includeQuestions = false)
    {
        LogGettingQuizzes(logger, pagingParams.PageNumber, pagingParams.PageSize, includeQuestions);

        IQueryable<Quiz> query = _context.Quizzes.AsNoTracking();

        query = query.Include(quiz => quiz.Image);

        if (includeQuestions)
        {
            query = query.Include(quiz => quiz.Questions)
                .ThenInclude(question => question.Choices);
        }

        return await PagedList<Quiz>.CreateAsync(query, pagingParams.PageNumber, pagingParams.PageSize);
    }

    public async Task<Quiz?> GetQuiz(int id, bool includeQuestions = false)
    {
        LogGettingQuiz(logger, id, includeQuestions);

        IQueryable<Quiz> query = _context.Quizzes;

        query = query.Include(quiz => quiz.Image);

        if (includeQuestions)
        {
            query = query.Include(quiz => quiz.Questions)
                .ThenInclude(question => question.Choices);
        }

        return await query.FirstOrDefaultAsync(quiz => quiz.Id == id);
    }
}