using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal sealed partial class ImageRepository(QuizDbContext context, ILogger<ImageRepository> logger) : Repository(context, logger), IImageRepository
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all images")]
    private static partial void LogGettingAllImages(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting Image {ImageId}")]
    private static partial void LogGettingImage(ILogger logger, int imageId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting Image by URL {Url}")]
    private static partial void LogGettingImageByUrl(ILogger logger, string url);
    
    public async Task<Image[]> GetImages()
    {
        LogGettingAllImages(logger);
        return await _context.Images.ToArrayAsync();
    }

    public async Task<Image?> GetImage(int imageId)
    {
        LogGettingImage(logger, imageId);
        return await _context.Images.FirstOrDefaultAsync(image => image.Id == imageId);
    }

    public async Task<Image?> GetImageByUrl(string url)
    {
        LogGettingImageByUrl(logger, url);
        return await _context.Images.FirstOrDefaultAsync(image => image.ImageUrl == url);
    }
}