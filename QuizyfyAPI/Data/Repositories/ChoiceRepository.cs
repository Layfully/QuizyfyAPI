using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QuizyfyAPI.Data
{
    public class ChoiceRepository : Repository, IChoiceRepository
    {

        public ChoiceRepository(QuizDbContext context, ILogger<ChoiceRepository> logger) : base(context, logger)
        {
        }

        public async Task<Choice[]> GetChoices(int questionId)
        {
            _logger.LogInformation($"Getting all choices for a Question for a Quiz");

            IQueryable<Choice> query = _context.Choices;

            query = query.Where(choice => choice.QuestionId == questionId);

            return await query.ToArrayAsync();
        }
        public async Task<Choice> GetChoice(int questionId, int choiceId)
        {
            _logger.LogInformation($"Getting one choice");

            IQueryable<Choice> query = _context.Choices;

            query = query.Where(choice => choice.QuestionId == questionId && choice.Id == choiceId);

            return await query.FirstOrDefaultAsync();
        }
    }
}
