using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizyfyAPI.Helpers;

namespace QuizyfyAPI.Data
{
    public class UserRepository : Repository, IUserRepository
    {

        public UserRepository(QuizDbContext context, ILogger<UserRepository> logger) : base(context, logger)
        {
        }

        public async Task<User> GetUserById(int userId)
        {

            _logger.LogInformation($"Getting user by id");

            IQueryable<User> query = _context.Users;

            query = query.Where(user => user.Id == userId);

            return await query.FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByUsername(string username)
        {
            _logger.LogInformation($"Getting user by id");

            IQueryable<User> query = _context.Users;

            query = query.Where(user => user.Username == username);

            return await query.FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByEmail(string email)
        {
            _logger.LogInformation($"Getting user by email");

            IQueryable<User> query = _context.Users;

            query = query.Where(user => user.Email == email);

            return await query.FirstOrDefaultAsync();
        }
        public async Task<User> Authenticate(string username, string password)
        {
            _logger.LogInformation($"Authenticating user");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            IQueryable<User> query = _context.Users.Include(s => s.RefreshToken);

            var user = await query.Where(userDb => userDb.Username == username).FirstOrDefaultAsync();

            
            if (user == null || !Hash.Verify(password, user?.PasswordHash, user?.PasswordSalt))
            {
                return null;
            }

            return user;
        }
    }
}
