using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Data.Repositories.Interfaces;

internal interface IRefreshTokenRepository : IRepository
{ 
    Task<RefreshToken?> GetRefreshToken(string refreshToken);
    
    void UpdateRefreshToken(RefreshToken refreshToken);
}