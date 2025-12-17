using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal sealed partial class LikeRepository(QuizDbContext context, ILogger<LikeRepository> logger) : Repository(context, logger), ILikeRepository
{
    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting Like for Quiz {QuizId} by User {UserId}")]
    private static partial void LogGettingLike(ILogger logger, int quizId, int userId);

    [LoggerMessage(
        Level = LogLevel.Information, 
        Message = "Getting all likes for Quiz {QuizId}")]
    private static partial void LogGettingLikes(ILogger logger, int quizId);
    
    public async Task<Like?> GetLike(int quizId, int userId)
    {
        LogGettingLike(logger, quizId, userId);
        
        return await _context.Likes
            .SingleOrDefaultAsync(like => like.QuizId == quizId && like.UserId == userId);
    }

    public async Task<Like[]> GetLikes(int quizId)
    {
        LogGettingLikes(logger, quizId);
        
        return await _context.Likes
            .Where(like => like.QuizId == quizId)
            .ToArrayAsync();
    }
}