﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QuizyfyAPI.Data
{
    public class QuestionRepository : Repository, IQuestionRepository
    {
        public QuestionRepository(QuizDbContext context, ILogger<QuestionRepository> logger) : base(context, logger)
        {
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
              .Where(question => question.QuizId == quizId);

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
