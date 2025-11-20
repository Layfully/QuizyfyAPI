using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Contracts.Responses.Pagination;
using QuizyfyAPI.Controllers;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;

namespace QuizyfyAPI.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<QuizService> _logger;
    
    public QuizService(
        IQuizRepository quizRepository, 
        IImageRepository imageRepository, 
        IMemoryCache cache,
        IMapper mapper,
        ILogger<QuizService> logger)
    {
        _quizRepository = quizRepository;
        _imageRepository = imageRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ObjectResult<QuizResponse>> Get(int id, bool includeQuestions)
    {
        string cacheKey = $"Quiz_{id}_{includeQuestions}";

        Quiz? quiz = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _quizRepository.GetQuiz(id, includeQuestions);
        });


        if (quiz is null)
        {
            return new ObjectResult<QuizResponse> { Errors = ["Couldn't find this quiz"] };
        }

        return new ObjectResult<QuizResponse> { Success = true, Found = true, Object = _mapper.Map<QuizResponse>(quiz) };
    }

    public async Task<ObjectResult<QuizResponse>> Create(QuizCreateRequest request)
    {
        Quiz? quiz = _mapper.Map<Quiz>(request);

        quiz.DateAdded = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            Image? image = await _imageRepository.GetImageByUrl(request.ImageUrl);
            
            if (image is not null)
            {
                quiz.Image = image;
            }
            else
            {
                _logger.LogWarning("Quiz created with ImageUrl '{Url}' but image not found in DB.", request.ImageUrl);
            }
        }

        _quizRepository.Add(quiz);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Set($"Quiz_{quiz.Id}_False", quiz, TimeSpan.FromMinutes(5));

            return new ObjectResult<QuizResponse> { Object = _mapper.Map<QuizResponse>(quiz), Found = true, Success = true };
        }

        return new ObjectResult<QuizResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<ObjectResult<QuizResponse>> Update(int quizId, QuizUpdateRequest request)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz is null)
        {
            return new ObjectResult<QuizResponse> { Errors = ["Couldn't find quiz this quiz"] };
        }

        _mapper.Map(request, quiz);
        
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

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Remove($"Quiz_{quizId}_True");
            _cache.Remove($"Quiz_{quizId}_False");
            
            _cache.Set($"Quiz_{quizId}_False", quiz, TimeSpan.FromMinutes(5));
            
            return new ObjectResult<QuizResponse> { Success = true, Found = true, Object = _mapper.Map<QuizResponse>(quiz) };
        }

        return new ObjectResult<QuizResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<DetailedResult> Delete(int quizId)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz is null)
        {
            return new DetailedResult { Errors = ["Couldn't find this quiz"] };
        }

        _quizRepository.Delete(quiz);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Remove($"Quiz_{quizId}_True");
            _cache.Remove($"Quiz_{quizId}_False");
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
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
            Items = _mapper.Map<List<QuizResponse>>(pagedList.List)
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
