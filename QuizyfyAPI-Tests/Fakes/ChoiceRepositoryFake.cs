using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace QuizyfyAPI_Tests.Fakes
{
    class ChoiceRepositoryFake : IChoiceRepository
    {
        private readonly QuizDbContext _context;

        public ChoiceRepositoryFake()
        {
            DbContextOptions<QuizDbContext> options = new DbContextOptionsBuilder<QuizDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _context = new QuizDbContext(options: options);
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

        public async Task<Choice> GetChoice(int questionId, int choiceId)
        {
            IQueryable<Choice> query = _context.Choices;

            query = query.Where(choice => choice.QuestionId == questionId && choice.Id == choiceId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Choice[]> GetChoices(int questionId)
        {
            IQueryable<Choice> query = _context.Choices;

            query = query.Where(choice => choice.QuestionId == questionId);

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
