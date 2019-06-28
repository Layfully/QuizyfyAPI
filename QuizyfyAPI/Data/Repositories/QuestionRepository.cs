using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizyfyAPI.Helpers;

namespace QuizyfyAPI.Data
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly QuizDbContext _context;
        private readonly ILogger<QuestionRepository> _logger;

        public QuestionRepository(QuizDbContext context, ILogger<QuestionRepository> logger)
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
        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<Question[]> GetQuestions(int quizId, bool includeChoices = false)
        {
            _logger.LogInformation($"Getting all Questions for a Quiz");

            IQueryable<Question> query = _context.Questions;

            if (includeChoices)
            {
                query = query.Include(question => question.Choices);
            }

            query = query
              .Where(question => question.QuizId == quizId)
              .OrderByDescending(question => question.Id);

            return await query.ToArrayAsync();
        }

        public async Task<Question> GetQuestion(int quizId, int questionId, bool includeChoices = false)
        {
            _logger.LogInformation($"Getting one Question for a Quiz");

            IQueryable<Question> query = _context.Questions;

            if (includeChoices)
            {
                query = query.Include(question => question.Choices);
            }

            query = query
              .Where(question => question.Id == questionId && question.QuizId == quizId);

            return await query.FirstOrDefaultAsync();
        }
    }
}
