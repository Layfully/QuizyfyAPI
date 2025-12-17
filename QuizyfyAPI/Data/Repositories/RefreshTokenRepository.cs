using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal sealed class RefreshTokenRepository(QuizDbContext context, ILogger<RefreshTokenRepository> logger) : Repository(context, logger), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetRefreshToken(string refreshToken)
    {
        return await _context.RefreshTokens
            .SingleOrDefaultAsync(storedRefreshToken => storedRefreshToken.Token == refreshToken);
    }

    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        _context.Update(refreshToken);
    }
}