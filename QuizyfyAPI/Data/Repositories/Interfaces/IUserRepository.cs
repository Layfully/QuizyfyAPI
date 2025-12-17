using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Data.Repositories.Interfaces;

internal interface IUserRepository : IRepository
{
    Task<User?> GetUserById(int userId);
    
    Task<User?> GetUserByUsername(string username);
    
    Task<User?> GetUserByEmail(string email);
    
    Task<User?> Authenticate(string username, string password);
}