using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal sealed partial class QuestionRepository(QuizDbContext context, ILogger<QuestionRepository> logger) : Repository(context, logger), IQuestionRepository
{
    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting all Questions for Quiz {QuizId}. IncludeChoices: {IncludeChoices}")]
    private static partial void LogGettingQuestions(ILogger logger, int quizId, bool includeChoices);

    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting Question {QuestionId} for Quiz {QuizId}. IncludeChoices: {IncludeChoices}")]
    private static partial void LogGettingQuestion(ILogger logger, int questionId, int quizId, bool includeChoices);
    
    public async Task<Question[]> GetQuestions(int quizId, bool includeChoices = false)
    {
        LogGettingQuestions(logger, quizId, includeChoices);
        
        IQueryable<Question> query = _context.Questions;

        query = query.Include(question => question.Image);

        if (includeChoices)
        {
            query = query.Include(question => question.Choices);
        }
        
        return await query
            .Where(question => question.QuizId == quizId)
            .ToArrayAsync();
    }

    public async Task<Question?> GetQuestion(int quizId, int questionId, bool includeChoices = false)
    {
        LogGettingQuestion(logger, questionId, quizId, includeChoices);
        
        IQueryable<Question> query = _context.Questions;

        query = query.Include(question => question.Image);

        if (includeChoices)
        {
            query = query.Include(question => question.Choices);
        }

        return await query
            .FirstOrDefaultAsync(question => question.Id == questionId && question.QuizId == quizId);
    }
}