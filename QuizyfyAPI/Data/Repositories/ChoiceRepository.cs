using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal sealed partial class ChoiceRepository(QuizDbContext context, ILogger<ChoiceRepository> logger) : Repository(context, logger), IChoiceRepository
{
    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting all choices for Question {QuestionId}")]
    private static partial void LogGettingChoices(ILogger logger, int questionId);

    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting Choice {ChoiceId} for Question {QuestionId}")]
    private static partial void LogGettingChoice(ILogger logger, int choiceId, int questionId);
    
    public async Task<Choice[]> GetChoices(int questionId)
    {
        LogGettingChoices(logger, questionId);
        
        return await _context.Choices
            .Where(choice => choice.QuestionId == questionId)
            .ToArrayAsync();
    }

    public async Task<Choice?> GetChoice(int questionId, int choiceId)
    {
        LogGettingChoice(logger, choiceId, questionId);
        return await _context.Choices
            .FirstOrDefaultAsync(choice => choice.QuestionId == questionId && choice.Id == choiceId);
    }
}
