using AutoMapper;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Options;

namespace QuizyfyAPI.Services;
public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly AppOptions _appOptions;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ImageService> _logger;
    
    private const string AllImagesCacheKey = "Images";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    
    public ImageService(
        IImageRepository imageRepository, 
        IMemoryCache cache, 
        IMapper mapper, 
        IOptions<AppOptions> appOptions, 
        IWebHostEnvironment webHostEnvironment,
        ILogger<ImageService> logger)
    {
        _imageRepository = imageRepository;
        _cache = cache;
        _mapper = mapper;
        _appOptions = appOptions.Value;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<ObjectResult<ImageResponse[]>> GetAll()
    {
        ICollection<Image>? images = await _cache.GetOrCreateAsync(AllImagesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _imageRepository.GetImages();
        });

        if (images is null || images.Count == 0)
        {
            return new ObjectResult<ImageResponse[]> { Errors = ["There are no images"] };
        }
        
        return new ObjectResult<ImageResponse[]> 
        { 
            Object = _mapper.Map<ImageResponse[]>(images), 
            Found = true, 
            Success = true 
        };    
    }

    public async Task<ObjectResult<ImageResponse>> Get(int id)
    {
        Image? image = await _cache.GetOrCreateAsync($"Image_{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _imageRepository.GetImage(id);
        });

        if (image is null)
        {
            return new ObjectResult<ImageResponse> { Errors = ["Couldn't find this image"] };
        }

        return new ObjectResult<ImageResponse> { Object = _mapper.Map<ImageResponse>(image), Found = true, Success = true };
    }

    public async Task<ObjectResult<ImageResponse>> Create(IFormFile file)
    {
        if (file?.Length > 0)
        {
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
                    _cache.Remove(AllImagesCacheKey);
                    return new ObjectResult<ImageResponse> { Success = true, Object = _mapper.Map<ImageResponse>(image) };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error during image creation. Cleaning up file.");
                TryDeletePhysicalFile(imageUrl);
                throw;
            }
        }

        return new ObjectResult<ImageResponse> { Errors = ["File couldn't be uploaded"] };
    }

    public async Task<ObjectResult<ImageResponse>> Update(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
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
                
                _cache.Set($"Image_{image.Id}", image, TimeSpan.FromMinutes(5));
                _cache.Remove(AllImagesCacheKey);
                
                return new ObjectResult<ImageResponse> 
                { 
                    Object = _mapper.Map<ImageResponse>(image), 
                    Found = true, 
                    Success = true 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error during image update. Cleaning up new file.");
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
            _cache.Remove($"Image_{id}");
            _cache.Remove(AllImagesCacheKey);
            return new DetailedResult { Found = true, Success = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    /// <summary>
    /// Safely attempts to delete a file from the web root. Logs on failure.
    /// </summary>
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

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted physical file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw, as the DB operation was already successful
            _logger.LogWarning(ex, "Failed to delete physical file for URL: {ImageUrl}", imageUrl);
        }
    }
    
    private async Task<string?> UploadFile(IFormFile file)
    {
        try
        {
            if (string.IsNullOrEmpty(file.FileName))
            {
                return null;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension uploaded: {Extension}", extension);
                return null;
            }
            
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            
            // Truncate original name if too long to avoid filesystem errors
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
            
            _logger.LogInformation("Uploaded file: {FilePath}", filePath);

            string dbUrl = $"{_appOptions.ServerPath.TrimEnd('/')}/images/quizzes/{uniqueFileName}";
            return dbUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            return null;
        }
    }
}
