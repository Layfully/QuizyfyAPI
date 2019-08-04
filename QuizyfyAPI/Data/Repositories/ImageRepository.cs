using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class ImageRepository : IImageRepository
    {
        private readonly QuizDbContext _context;
        private readonly ILogger<ImageRepository> _logger;
        public ImageRepository(QuizDbContext context, ILogger<ImageRepository> logger)
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
        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }
        public async Task<Image[]> GetImages()
        {
            _logger.LogInformation($"Getting all images");

            IQueryable<Image> query = _context.Images;

            return await query.ToArrayAsync();
        }
        public async Task<Image> GetImage(int imageId)
        {
            _logger.LogInformation($"Getting one image");

            IQueryable<Image> query = _context.Images;

            query = query.Where(image => image.Id == imageId);

            return await query.FirstOrDefaultAsync();
        }
    }
}
