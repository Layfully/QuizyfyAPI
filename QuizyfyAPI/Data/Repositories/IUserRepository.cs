namespace QuizyfyAPI.Data;
public interface IUserRepository : IRepository
{
    Task<User> GetUserById(int userId);
    Task<User> GetUserByUsername(string username);
    Task<User> GetUserByEmail(string email);
    Task<User> Authenticate(string username, string password);
}

