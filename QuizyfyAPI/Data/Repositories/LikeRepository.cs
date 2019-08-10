using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data.Repositories
{
    public class LikeRepository : Repository, ILikeRepository
    {
        public LikeRepository(QuizDbContext context, ILogger<LikeRepository> logger) : base(context, logger)
        {
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

    }
}
