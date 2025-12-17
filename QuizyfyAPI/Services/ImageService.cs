using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Mappers;
using QuizyfyAPI.Options;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Services;

internal sealed partial class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly HybridCache _hybridCache;
    private readonly AppOptions _appOptions;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IOutputCacheStore _outputCache;
    private readonly ILogger<ImageService> _logger;
    
    private const string AllImagesCacheKey = "Images";
    private static readonly SearchValues<string> _allowedExtensions = 
        SearchValues.Create([".jpg", ".jpeg", ".png", ".gif", ".webp"], StringComparison.OrdinalIgnoreCase);
    
    public ImageService(
        IImageRepository imageRepository, 
        HybridCache hybridCache, 
        IOptions<AppOptions> appOptions, 
        IWebHostEnvironment webHostEnvironment,
        IOutputCacheStore outputCache,
        ILogger<ImageService> logger)
    {
        _imageRepository = imageRepository;
        _hybridCache = hybridCache;
        _appOptions = appOptions.Value;
        _webHostEnvironment = webHostEnvironment;
        _outputCache = outputCache;
        _logger = logger;
    }
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Database error during image {Operation}. Cleaning up file.")]
    private static partial void LogDatabaseError(ILogger logger, Exception ex, string operation);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleted physical file: {FilePath}")]
    private static partial void LogDeletedFile(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to delete physical file for URL: {ImageUrl}")]
    private static partial void LogDeleteFileError(ILogger logger, Exception ex, string imageUrl);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid file extension uploaded: {Extension}")]
    private static partial void LogInvalidExtension(ILogger logger, string extension);

    [LoggerMessage(Level = LogLevel.Information, Message = "Uploaded file: {FilePath}")]
    private static partial void LogUploadedFile(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error uploading file: {FileName}")]
    private static partial void LogUploadError(ILogger logger, Exception ex, string fileName);
    
    public async Task<ObjectResult<ImageResponse[]>> GetAll()
    {
        Image[] images = await _hybridCache.GetOrCreateAsync(
            AllImagesCacheKey, 
            async _ => await _imageRepository.GetImages(),
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(10) }
        );
        
        if (images.Length == 0)
        {
            return new ObjectResult<ImageResponse[]> { Errors = ["There are no images"] };
        }
        
        return new ObjectResult<ImageResponse[]> 
        { 
            Object = images.Select(i => i.ToResponse()).ToArray(), 
            Found = true, 
            Success = true 
        };    
    }

    public async Task<ObjectResult<ImageResponse>> Get(int id)
    {
        Image? image = await _hybridCache.GetOrCreateAsync(
            $"Image_{id}", 
            async _ => await _imageRepository.GetImage(id),
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) }
        );

        if (image is null)
        {
            return new ObjectResult<ImageResponse> { Errors = ["Couldn't find this image"] };
        }

        return new ObjectResult<ImageResponse> 
        { 
            Object = image.ToResponse(), 
            Found = true, 
            Success = true 
        };
    }

    public async Task<ObjectResult<ImageResponse>> Create(IFormFile file)
    {
        if (file.Length == 0)
        {
            return new ObjectResult<ImageResponse> { Errors = ["File is empty"] };
        }
        
        string? imageUrl = await UploadFile(file);

        if (string.IsNullOrEmpty(imageUrl))
        {
            return new ObjectResult<ImageResponse> { Errors = ["Server failed to save the file"] };
        }

        Image image = new()
        {
            ImageUrl = imageUrl
        };

        _imageRepository.Add(image);

        try 
        {
            if (await _imageRepository.SaveChangesAsync())
            {
                await _hybridCache.RemoveAsync(AllImagesCacheKey);
                await _outputCache.EvictByTagAsync("images", CancellationToken.None); 
                    
                await _hybridCache.SetAsync(
                    $"Image_{image.Id}", 
                    image,
                    options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) }
                );                    
                    
                return new ObjectResult<ImageResponse> 
                { 
                    Success = true, 
                    Object = image.ToResponse() 
                };
            }
        }
        catch (Exception ex)
        {
            LogDatabaseError(_logger, ex, "creation");
            TryDeletePhysicalFile(imageUrl);
            throw;
        }

        return new ObjectResult<ImageResponse> { Errors = ["File couldn't be uploaded"] };
    }

    public async Task<ObjectResult<ImageResponse>> Update(int id, IFormFile file)
    {
        if (file.Length == 0)
        {
            return new ObjectResult<ImageResponse> { Errors = ["New file is empty"] };
        }
        
        Image? image = await _imageRepository.GetImage(id);

        if (image is null)
        {
            return new ObjectResult<ImageResponse> { Errors = ["Couldn't find the choice to update"] };
        }
        
        // Capture old URL to delete later
        string oldImageUrl = image.ImageUrl;
        
        string? newUrl = await UploadFile(file);
        
        if (string.IsNullOrEmpty(newUrl))
        {
            return new ObjectResult<ImageResponse> { Errors = ["Server failed to save the new file"] };
        }
        
        image.ImageUrl = newUrl;
        _imageRepository.Update(image);
        
        try
        {
            if (await _imageRepository.SaveChangesAsync())
            {
                TryDeletePhysicalFile(oldImageUrl);
                
                await _hybridCache.SetAsync(
                    $"Image_{image.Id}", 
                    image, 
                    options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) }
                );
                
                await _hybridCache.RemoveAsync(AllImagesCacheKey);
                await _outputCache.EvictByTagAsync("images", CancellationToken.None);
                await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
                
                return new ObjectResult<ImageResponse> 
                { 
                    Object = image.ToResponse(), 
                    Found = true, 
                    Success = true 
                };
            }
        }
        catch (Exception ex)
        {
            LogDatabaseError(_logger, ex, "update");
            TryDeletePhysicalFile(newUrl);
            throw; 
        }

        TryDeletePhysicalFile(newUrl);
        return new ObjectResult<ImageResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<DetailedResult> Delete(int id)
    {
        Image? image = await _imageRepository.GetImage(id);

        if (image is null)
        {
            return new DetailedResult { Errors = ["Couldn't find the image to delete"] };
        }

        _imageRepository.Delete(image);

        if (await _imageRepository.SaveChangesAsync())
        {
            TryDeletePhysicalFile(image.ImageUrl);
            await _hybridCache.RemoveAsync($"Image_{id}");
            await _hybridCache.RemoveAsync(AllImagesCacheKey);
            
            await _outputCache.EvictByTagAsync("images", CancellationToken.None);
            await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
            return new DetailedResult { Found = true, Success = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    /// <summary>
    /// Safely attempts to delete a file from the web root. Logs on failure.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private void TryDeletePhysicalFile(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        try
        {
            string fileName = Path.GetFileName(imageUrl);
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "quizzes", fileName);

            if (!File.Exists(filePath))
            {
                return;
            }
            
            File.Delete(filePath);
            LogDeletedFile(_logger, filePath);
        }
        catch (Exception ex)
        {
            // Log but don't throw, as the DB operation was already successful
            LogDeleteFileError(_logger, ex, imageUrl);
        }
    }
    
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private async Task<string?> UploadFile(IFormFile file)
    {
        try
        {
            if (string.IsNullOrEmpty(file.FileName))
            {
                return null;
            }

            string extension = Path.GetExtension(file.FileName);
            
            if (!_allowedExtensions.Contains(extension))
            {
                LogInvalidExtension(_logger, extension);
                return null;
            }
            
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            
            // Truncate the original name if too long to avoid filesystem errors
            if (fileNameWithoutExt.Length > 50) 
            {
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, 50);
            }
            string uniqueFileName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{extension}";
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "quizzes");
            
            Directory.CreateDirectory(folderPath); 
            
            string filePath = Path.Combine(folderPath, uniqueFileName);
            
            await using FileStream fileStream = File.Create(filePath);
            await file.CopyToAsync(fileStream);
            
            LogUploadedFile(_logger, filePath);

            string dbUrl = $"{_appOptions.ServerPath.TrimEnd('/')}/images/quizzes/{uniqueFileName}";
            return dbUrl;
        }
        catch (Exception ex)
        {
            LogUploadError(_logger, ex, file.FileName);
            return null;
        }
    }
}
