using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizyfyAPI.Contracts.Responses.Pagination;

namespace QuizyfyAPI_Tests.Fakes
{
    class QuizRepositoryFake : IQuizRepository
    {
        private readonly QuizDbContext _context;

        public QuizRepositoryFake() { }
        
         //   DbContextOptions<QuizDbContext> options = new DbContextOptionsBuilder<QuizDbContext>()
              //  .//UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

         //   _context = new QuizDbContext(options: options);
        

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

        public async Task<Quiz> GetQuiz(int id, bool includeQuestions = false)
        {
            IQueryable<Quiz> _query = _context.Quizzes;

            _query = _query.Where(quiz => quiz.Id == id).OrderBy(q => q.Id);

            _query = _query.OrderBy(q => q.Id);

            return await _query.FirstOrDefaultAsync();
        }

        public PagedList<Quiz> GetQuizzes(PagingParams pagingParams, bool includeQuestions = false)
        {
            IQueryable<Quiz> _query = _context.Quizzes;

            _query = _query.OrderBy(q => q.Id);

            throw new NotImplementedException(); // returnawait _query.ToArrayAsync();
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
