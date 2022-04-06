namespace QuizyfyAPI.Data;
public interface IRefreshTokenRepository : IRepository
{
    Task<RefreshToken?> GetRefreshToken(string refreshToken);
    void UpdateRefreshToken(RefreshToken refreshToken);
}
