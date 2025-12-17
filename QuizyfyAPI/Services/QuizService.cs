using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Hybrid;
using QuizyfyAPI.Contracts.Responses.Pagination;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Mappers;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Services;

internal sealed partial class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IImageRepository _imageRepository;
    private readonly HybridCache _hybridCache;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<QuizService> _logger;
    private readonly IOutputCacheStore _outputCache;
    
    public QuizService(
        IQuizRepository quizRepository, 
        IImageRepository imageRepository, 
        HybridCache hybridCache,
        TimeProvider timeProvider,
        IOutputCacheStore outputCache,
        ILogger<QuizService> logger)
    {
        _quizRepository = quizRepository;
        _imageRepository = imageRepository;
        _hybridCache = hybridCache;
        _timeProvider = timeProvider;
        _outputCache = outputCache;
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Quiz created with ImageUrl '{Url}' but image not found in DB.")]
    private static partial void LogImageNotFound(ILogger logger, string url);
    
    public async Task<ObjectResult<QuizResponse>> Get(int id, bool includeQuestions)
    {
        string cacheKey = $"Quiz_{id}_{includeQuestions}";

        Quiz? quiz = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async _ => await _quizRepository.GetQuiz(id, includeQuestions),
            options: new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Flags = HybridCacheEntryFlags.None,
            },
            tags: [$"Quiz:{id}"] 
        );

        if (quiz is null)
        {
            return new ObjectResult<QuizResponse> { Errors = ["Couldn't find this quiz"] };
        }

        return new ObjectResult<QuizResponse> 
        { 
            Success = true, 
            Found = true, 
            Object = quiz.ToResponse() 
        };    
    }

    public async Task<ObjectResult<QuizResponse>> Create(QuizCreateRequest request)
    {
        Quiz quiz = request.ToEntity();

        quiz.DateAdded = _timeProvider.GetUtcNow().DateTime; 

        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            Image? image = await _imageRepository.GetImageByUrl(request.ImageUrl);
            
            if (image is not null)
            {
                quiz.Image = image;
            }
            else
            {
                LogImageNotFound(_logger, request.ImageUrl);
            }
        }

        _quizRepository.Add(quiz);

        if (!await _quizRepository.SaveChangesAsync())
        {
            return new ObjectResult<QuizResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
        }

        await _hybridCache.SetAsync(
            $"Quiz_{quiz.Id}_False", 
            quiz, 
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quiz.Id}"]
        );

        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
        
        return new ObjectResult<QuizResponse> 
        { 
            Object = quiz.ToResponse(), 
            Found = true, 
            Success = true 
        };
    }

    public async Task<ObjectResult<QuizResponse>> Update(int quizId, QuizUpdateRequest request)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz is null)
        {
            return new ObjectResult<QuizResponse> { Errors = ["Couldn't find quiz this quiz"] };
        }

        quiz.UpdateFrom(request);
        
        if (request.ImageId.HasValue && request.ImageId != 0)
        {
            Image? newImage = await _imageRepository.GetImage(request.ImageId.Value);
            
            if (newImage is null)
            {
                return new ObjectResult<QuizResponse> { Errors = ["Invalid ImageId provided"] };
            }
            
            quiz.Image = newImage;
        }
        
        _quizRepository.Update(quiz);

        if (!await _quizRepository.SaveChangesAsync())
        {
            return new ObjectResult<QuizResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
        }

        await _hybridCache.RemoveByTagAsync($"Quiz:{quizId}");
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);

        await _hybridCache.SetAsync(
            $"Quiz_{quizId}_False", 
            quiz,
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quizId}"]
        );            
            
        return new ObjectResult<QuizResponse> 
        { 
            Success = true, 
            Found = true, 
            Object = quiz.ToResponse() 
        };
    }

    public async Task<DetailedResult> Delete(int quizId)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz is null)
        {
            return new DetailedResult { Errors = ["Couldn't find this quiz"] };
        }

        _quizRepository.Delete(quiz);

        if (!await _quizRepository.SaveChangesAsync())
        {
            return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
        }

        await _hybridCache.RemoveByTagAsync($"Quiz:{quizId}");
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
            
        return new DetailedResult { Success = true, Found = true };
    }

    public async Task<ObjectResult<QuizListResponse>> GetAll(PagingParams pagingParams, HttpContext httpContext)
    {
        var pagedList = await _quizRepository.GetQuizzes(pagingParams);

        if (pagedList.List.Count == 0)
        {
            return new ObjectResult<QuizListResponse> { Errors = ["No quizzes found"] };
        }

        QuizListResponse output = new()
        {
            Paging = pagedList.GetHeader(),
            Links = GetLinks(pagedList, httpContext),
            Items = pagedList.List.Select(q => q.ToResponse()).ToList()
        };

        return new ObjectResult<QuizListResponse> { Success = true, Found = true, Object = output };
    }

    private static List<LinkInfo> GetLinks(PagedList<Quiz> list, HttpContext httpContext)
    {
        List<LinkInfo> links = [];

        if (list.HasPreviousPage)
        {
            links.Add(CreateLink(httpContext, list.PreviousPageNumber, list.PageSize, "previousPage", "GET"));
        }

        links.Add(CreateLink(httpContext, list.PageNumber, list.PageSize, "self", "GET"));

        if (list.HasNextPage)
        {
            links.Add(CreateLink(httpContext, list.NextPageNumber, list.PageSize, "nextPage", "GET"));
        }

        return links;
    }

    private static LinkInfo CreateLink(HttpContext httpContext, int pageNumber, int pageSize, string rel, string method)
    {
        return new LinkInfo
        {
            Href = $"{httpContext.Request.Path}?PageNumber={pageNumber}&PageSize={pageSize}",
            Rel = rel,
            Method = method
        };
    }
}
