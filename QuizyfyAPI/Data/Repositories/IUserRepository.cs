using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public interface IUserRepository : IRepository
    {
        Task<User> GetUserById(int userId);
        Task<User> GetUserByUsername(string username);
        Task<User> Authenticate(string username, string password);
    }
}

