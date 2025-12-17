using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Helpers;

namespace QuizyfyAPI.Data.Repositories;

internal sealed partial class UserRepository(QuizDbContext context, ILogger<UserRepository> logger) : Repository(context, logger), IUserRepository
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Getting user {UserId}")]
    private static partial void LogGettingUserById(ILogger logger, int userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting user by username {Username}")]
    private static partial void LogGettingUserByUsername(ILogger logger, string username);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting user by email {Email}")]
    private static partial void LogGettingUserByEmail(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Authenticating user {Username}")]
    private static partial void LogAuthenticatingUser(ILogger logger, string username);
    
    public async Task<User?> GetUserById(int userId)
    {
        LogGettingUserById(logger, userId);
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        LogGettingUserByUsername(logger, username);
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        LogGettingUserByEmail(logger, email);
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User?> Authenticate(string username, string password)
    {
        LogAuthenticatingUser(logger, username);
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        User? user = await _context.Users
            .Include(u => u.RefreshToken)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return null;
        }

        if (!Hash.Verify(password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        return user;
    }
}