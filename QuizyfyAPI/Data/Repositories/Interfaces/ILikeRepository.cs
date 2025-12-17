using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Data.Repositories.Interfaces;

internal interface ILikeRepository : IRepository
{
    Task<Like[]> GetLikes(int quizId);
    
    Task<Like?> GetLike(int quizId, int userId);
}