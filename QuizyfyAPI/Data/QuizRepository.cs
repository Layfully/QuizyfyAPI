using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizyfyAPI.Helpers;

namespace QuizyfyAPI.Data
{
    public class QuizRepository : IQuizRepository
    {

        private readonly QuizDbContext _context;
        private readonly ILogger<QuizRepository> _logger;

        public QuizRepository(QuizDbContext context, ILogger<QuizRepository> logger)
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

        public async Task<Quiz[]> GetQuizzes(bool includeQuestions = false)
        {
            _logger.LogInformation($"Getting all quizzes");

            IQueryable<Quiz> query = _context.Quizzes;

            if (includeQuestions)
            {
                _logger.LogInformation($"With questions");
                query = query.Include(quiz => quiz.Questions).ThenInclude(question => question.Choices);
            }

            return await query.ToArrayAsync();
        }

        public async Task<Quiz> GetQuiz(int id, bool includeQuestions = false)
        {
            _logger.LogInformation($"Getting one quiz");

            IQueryable<Quiz> query = _context.Quizzes;

            if (includeQuestions)
            {
                _logger.LogInformation($"With questions");
                query = query.Include(quiz => quiz.Questions).ThenInclude(question => question.Choices);
            }

            query = query.Where(quiz => quiz.Id == id);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Question[]> GetQuestions(int quizId, bool includeChoices = false)
        {
            _logger.LogInformation($"Getting all Questions for a Quiz");

            IQueryable<Question> query = _context.Questions;

            if (includeChoices)
            {
                query = query.Include(question => question.Choices);
            }

            query = query
              .Where(question => question.QuizId == quizId)
              .OrderByDescending(question => question.Id);

            return await query.ToArrayAsync();
        }

        public async Task<Question> GetQuestion(int quizId, int questionId, bool includeChoices = false)
        {
            _logger.LogInformation($"Getting one Question for a Quiz");

            IQueryable<Question> query = _context.Questions;

            if (includeChoices)
            {
                query = query.Include(question => question.Choices);
            }

            query = query
              .Where(question => question.Id == questionId && question.QuizId == quizId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Choice[]> GetChoices(int quizId, int questionId)
        {
            _logger.LogInformation($"Getting all choices for a Question for a Quiz");

            IQueryable<Choice> query = _context.Choices;
            IQueryable<Question> questionsQuery = _context.Questions;

            questionsQuery = questionsQuery.Where(question => question.QuizId == quizId && question.Id == questionId);


            if (await questionsQuery.FirstOrDefaultAsync() == null)
            {
                return null;
            }

            query = query.Where(choice => choice.QuestionId == questionId);

            return await query.ToArrayAsync();
        }
        [Obsolete("Think about using repository methods for gtting question here")]
        public async Task<Choice> GetChoice(int quizId, int questionId, int choiceId)
        {
            _logger.LogInformation($"Getting one choice");

            IQueryable<Choice> query = _context.Choices;
            IQueryable<Question> questionsQuery = _context.Questions;

            questionsQuery = questionsQuery.Where(question => question.QuizId == quizId && question.Id == questionId);

            if (await questionsQuery.FirstOrDefaultAsync() == null)
            {
                return null;
            }

            query = query.Where(choice => choice.Id == choiceId && choice.QuestionId == questionId);

            return await query.FirstOrDefaultAsync();
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
