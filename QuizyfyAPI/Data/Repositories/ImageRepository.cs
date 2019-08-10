﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class ImageRepository : Repository, IImageRepository
    {
        public ImageRepository(QuizDbContext context, ILogger<ImageRepository> logger) : base(context, logger)
        {
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
