using Microsoft.EntityFrameworkCore;

namespace QuizyfyAPI.Data;
public class RefreshTokenRepository : Repository, IRefreshTokenRepository
{
    public RefreshTokenRepository(QuizDbContext context, ILogger<RefreshTokenRepository> logger) : base(context, logger)
    {
    }

    public Task<RefreshToken?> GetRefreshToken(string refreshToken)
    {
        return _context.RefreshTokens.SingleOrDefaultAsync(storedRefreshToken => storedRefreshToken.Token == refreshToken);
    }

    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        _context.Update(refreshToken);
    }
}
