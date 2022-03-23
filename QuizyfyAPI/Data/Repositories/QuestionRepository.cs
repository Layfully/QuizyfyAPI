using Microsoft.EntityFrameworkCore;

namespace QuizyfyAPI.Data;
public class QuestionRepository : Repository, IQuestionRepository
{
    public QuestionRepository(QuizDbContext context, ILogger<QuestionRepository> logger) : base(context, logger)
    {
    }

    public Task<Question[]> GetQuestions(int quizId, bool includeChoices = false)
    {
        _logger.LogInformation($"Getting all Questions for a Quiz");

        IQueryable<Question> query = _context.Questions;

        if (includeChoices)
        {
            query = query.Include(question => question.Choices);
        }

        query = query
          .Where(question => question.QuizId == quizId);

        return query.ToArrayAsync();
    }

    public Task<Question> GetQuestion(int quizId, int questionId, bool includeChoices = false)
    {
        _logger.LogInformation($"Getting one Question for a Quiz");

        IQueryable<Question> query = _context.Questions;

        if (includeChoices)
        {
            query = query.Include(question => question.Choices);
        }

        query = query
          .Where(question => question.Id == questionId && question.QuizId == quizId);

        return query.FirstOrDefaultAsync();
    }
}
