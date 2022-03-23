namespace QuizyfyAPI.Data;
public interface ILikeRepository : IRepository
{
    Task<Like[]> GetLikes(int quizId);
    Task<Like> GetLike(int quizId, int userId);
}
