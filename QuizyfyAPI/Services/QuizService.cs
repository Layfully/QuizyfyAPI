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

    public QuizService(IQuizRepository quizRepository, IImageRepository imageRepository, IMemoryCache cache,
        IMapper mapper)
    {
        _quizRepository = quizRepository;
        _imageRepository = imageRepository;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<ObjectResult<QuizResponse>> Get(int id, bool includeQuestions)
    {
        string cacheKey = $"Quiz_{id}";

        if (!_cache.TryGetValue(cacheKey, out Quiz? quiz))
        {
            quiz = await _quizRepository.GetQuiz(id, includeQuestions);

            if (quiz is not null)
            {
                _cache.Set(cacheKey, quiz, TimeSpan.FromMinutes(5));
            }
        }

        if (quiz is null)
        {
            return new ObjectResult<QuizResponse> { Errors = ["Couldn't find this quiz"] };
        }

        return new ObjectResult<QuizResponse>
            { Success = true, Found = true, Object = _mapper.Map<QuizResponse>(quiz) };
    }

    public async Task<ObjectResult<QuizResponse>> Create(QuizCreateRequest request)
    {
        Quiz? quiz = _mapper.Map<Quiz>(request);

        quiz.DateAdded = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            quiz.Image = await _imageRepository.GetImageByUrl(request.ImageUrl);
        }

        _quizRepository.Add(quiz);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Set($"Quiz_{quiz.Id}", quiz, TimeSpan.FromMinutes(5));
            // _cache.Remove("All_Quizzes"); TODO: Remove all quizzes cache if implemented

            return new ObjectResult<QuizResponse>
                { Object = _mapper.Map<QuizResponse>(quiz), Found = true, Success = true };
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

        _quizRepository.Update(quiz);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Set($"Quiz_{quizId}", quiz, TimeSpan.FromMinutes(5));
            //TODO: _cache.Remove($"Quizzes");
            return new ObjectResult<QuizResponse>
                { Success = true, Found = true, Object = _mapper.Map<QuizResponse>(quiz) };
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
            _cache.Remove($"Quiz_{quizId}");
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<ObjectResult<QuizListResponse>> GetAll(PagingParams pagingParams, HttpResponse response,
        HttpContext httpContext)
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

    private List<LinkInfo> GetLinks(PagedList<Quiz> list, HttpContext httpContext)
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

    private LinkInfo CreateLink(HttpContext httpContext, int pageNumber, int pageSize, string rel, string method)
    {
        return new LinkInfo
        {
            Href = $"{httpContext.Request.Path}?PageNumber={pageNumber}&PageSize={pageSize}",
            Rel = rel,
            Method = method
        };
    }
}
