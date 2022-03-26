using Microsoft.EntityFrameworkCore;

namespace QuizyfyAPI.Data;
public class ChoiceRepository : Repository, IChoiceRepository
{
    public ChoiceRepository(QuizDbContext context, ILogger<ChoiceRepository> logger) : base(context, logger)
    {
    }

    public Task<Choice[]> GetChoices(int questionId)
    {
        _logger.LogInformation($"Getting all choices for a Question for a Quiz");

        IQueryable<Choice> query = _context.Choices;

        query = query.Where(choice => choice.QuestionId == questionId);

        return query.ToArrayAsync();
    }

    public Task<Choice> GetChoice(int questionId, int choiceId)
    {
        _logger.LogInformation($"Getting one choice");

        IQueryable<Choice> query = _context.Choices;

        query = query.Where(choice => choice.QuestionId == questionId && choice.Id == choiceId);

        return query.FirstOrDefaultAsync();
    }
}
