using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QuizyfyAPI.Data
{
    public class ChoiceRepository : IChoiceRepository
    {
        private readonly QuizDbContext _context;
        private readonly ILogger<ChoiceRepository> _logger;

        public ChoiceRepository(QuizDbContext context, ILogger<ChoiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add<T>(T entity) where T : class
        {
            _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            _context.Add(entity);
        }
        public void Delete<T>(T entity) where T : class
        {
            _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
            _context.Remove(entity);
        }
        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
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
