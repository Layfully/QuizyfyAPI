using Microsoft.EntityFrameworkCore;

namespace QuizyfyAPI.Data;
public class ImageRepository : Repository, IImageRepository
{
    public ImageRepository(QuizDbContext context, ILogger<ImageRepository> logger) : base(context, logger)
    {
    }

    public Task<Image[]> GetImages()
    {
        _logger.LogInformation($"Getting all images");

        IQueryable<Image> query = _context.Images;

        return query.ToArrayAsync();
    }

    public Task<Image> GetImage(int imageId)
    {
        _logger.LogInformation($"Getting one image");

        IQueryable<Image> query = _context.Images;

        query = query.Where(image => image.Id == imageId);

        return query.FirstOrDefaultAsync();
    }

    public Task<Image> GetImageByUrl(string url)
    {
        _logger.LogInformation($"Getting one image");

        IQueryable<Image> query = _context.Images;

        query = query.Where(image => image.ImageUrl == url);

        return query.FirstOrDefaultAsync();
    }
}
