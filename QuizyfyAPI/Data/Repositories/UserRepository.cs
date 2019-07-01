using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizyfyAPI.Helpers;

namespace QuizyfyAPI.Data
{
    public class UserRepository : IUserRepository
    {

        private readonly QuizDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(QuizDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add<T>(T entity) where T : class
        {
            _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
            _context.Remove(entity);
        }
        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
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

        public async Task<User> Authenticate(string username, string password)
        {
            _logger.LogInformation($"Authenticating user");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            IQueryable<User> query = _context.Users;

            var user = await query.Where(userDb => userDb.Username == username).FirstOrDefaultAsync();

            
            if (user == null || !PasswordHash.Verify(password, user?.PasswordHash, user?.PasswordSalt))
            {
                return null;
            }

            return user;
        }
    }
}
