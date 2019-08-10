using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class RefreshTokenRepository : Repository, IRefreshTokenRepository
    {
        public RefreshTokenRepository(QuizDbContext context, ILogger<RefreshTokenRepository> logger) : base(context, logger)
        {
        }

        public async Task<RefreshToken> GetRefreshToken(string refreshToken)
        {
            return await _context.RefreshTokens.SingleOrDefaultAsync(storedRefreshToken => storedRefreshToken.Token == refreshToken);
        }

        public void UpdateRefreshToken(RefreshToken refreshToken)
        {
            _context.Update(refreshToken);
        }
    }
}
