using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly QuizDbContext _context;
        private readonly ILogger<LikeRepository> _logger;

        public LikeRepository(QuizDbContext context, ILogger<LikeRepository> logger)
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

        public async Task<Like> GetLike(int quizId, int userId)
        {
            _logger.LogInformation($"Getting one like for a Quiz");

            IQueryable<Like> query = _context.Likes;

            return await query.SingleOrDefaultAsync(like => like.QuizId == quizId && like.UserId == userId);

        }

        public async Task<Like[]> GetLikes(int quizId)
        {
            _logger.LogInformation($"Getting all likes for a Quiz");

            IQueryable<Like> query = _context.Likes;

            query = query.Where(like => like.QuizId == quizId);

            return await query.ToArrayAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

    }
}
