using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public interface ILikeRepository : IRepository
    {
        Task<Like[]> GetLikes(int quizId);
        Task<Like> GetLike(int quizId, int userId);
    }
}
