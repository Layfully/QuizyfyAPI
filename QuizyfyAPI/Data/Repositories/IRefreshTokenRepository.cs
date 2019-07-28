using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public interface IRefreshTokenRepository : IRepository
    {
        Task<RefreshToken> GetRefreshToken(string refreshToken);
        void UpdateRefreshToken(RefreshToken refreshToken);
    }
}
