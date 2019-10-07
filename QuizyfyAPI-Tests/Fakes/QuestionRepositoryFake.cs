using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace QuizyfyAPI_Tests.Fakes
{
    class QuestionRepositoryFake : IQuestionRepository
    {
        private readonly QuizDbContext _context;

        public QuestionRepositoryFake()
        {
           // DbContextOptions<QuizDbContext> options = new DbContextOptionsBuilder<QuizDbContext>()
             //   .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            //_context = new QuizDbContext(options: options);
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public void Empty<T>() where T : class
        {
            foreach (var entity in _context.Set<T>())
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }
        }

        public async Task<Question> GetQuestion(int quizId, int questionId, bool includeChoices = false)
        {
            IQueryable<Question> query = _context.Questions;

            if (includeChoices)
            {
                query = query.Include(question => question.Choices);
            }

            query = query
                    .Where(question => question.Id == questionId && question.QuizId == quizId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Question[]> GetQuestions(int quizId, bool includeChoices = false)
        {
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

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }
    }
}
