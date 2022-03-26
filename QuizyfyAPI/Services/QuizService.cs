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
    private readonly IQuestionService _questionService;

    public QuizService(IQuizRepository quizRepository, IImageRepository imageRepository, IMemoryCache cache, IMapper mapper, IQuestionService questionService)
    {
        _quizRepository = quizRepository;
        _imageRepository = imageRepository;
        _cache = cache;
        _mapper = mapper;
        _questionService = questionService;
    }

    public async Task<ObjectResult<QuizResponse>> Get(int id, bool includeQuestions)
    {
        if (!_cache.TryGetValue("Quizzes", out Quiz quiz))
        {
            quiz = await _quizRepository.GetQuiz(id, includeQuestions);
            _cache.Set($"Quiz {id}", quiz);
        }

        if (quiz == null)
        {
            return new ObjectResult<QuizResponse> { Errors = new[] { "Couldn't find this quiz" } };
        }
        return new ObjectResult<QuizResponse> { Success = true, Found = true, Object = _mapper.Map<QuizResponse>(quiz) };
    }

    public async Task<ObjectResult<QuizResponse>> Create(QuizCreateRequest request)
    {
        var quiz = _mapper.Map<Quiz>(request);

        if (quiz != null)
        {
            quiz.DateAdded = DateTime.Now;
            _quizRepository.Add(quiz);
        }

        quiz.Image = await _imageRepository.GetImageByUrl(request.ImageUrl);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Set($"Quiz {quiz.Id}", quiz);
            _cache.Remove($"Quizzes");

            return new ObjectResult<QuizResponse> { Object = _mapper.Map<QuizResponse>(quiz), Found = true, Success = true };
        }
        return new ObjectResult<QuizResponse> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }

    public async Task<ObjectResult<QuizResponse>> Update(int quizId, QuizUpdateRequest request)
    {
        var quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz == null)
        {
            return new ObjectResult<QuizResponse> { Errors = new[] { "Couldn't find quiz this quiz" } };
        }

        _quizRepository.Update(quiz);

        quiz = _mapper.Map<Quiz>(request);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Set($"Quiz {quizId}", quiz);
            _cache.Remove($"Quizzes");
            return new ObjectResult<QuizResponse> { Success = true, Found = true, Object = _mapper.Map<QuizResponse>(quiz) };
        }
        return new ObjectResult<QuizResponse> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }

    public async Task<DetailedResult> Delete(int quizId)
    {
        var quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz == null)
        {
            return new DetailedResult { Errors = new[] { "Couldn't find this quiz" } };
        }

        _quizRepository.Delete(quiz);

        if (await _quizRepository.SaveChangesAsync())
        {
            _cache.Remove($"Quiz {quizId}");
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }

    public Task<ObjectResult<QuizListResponse>> GetAll(PagingParams pagingParams, HttpResponse response, HttpContext httpContext)
    {
        return Task.Run(() =>
        {
            PagedList<Quiz> obj = _quizRepository.GetQuizzes(pagingParams);

            if (obj.List.Count == 0)
            {
                return new ObjectResult<QuizListResponse> { Errors = new[] { "Couldn't find this quiz" } };
            }

            var output = new QuizListResponse
            {
                Paging = obj.GetHeader(),
                Links = GetLinks(obj, httpContext),
                Items = _mapper.Map<List<QuizResponse>>(obj.List)
            };

            return new ObjectResult<QuizListResponse> { Success = true, Found = true, Object = output };
        });
    }

    private List<LinkInfo> GetLinks(PagedList<Quiz> list, HttpContext HttpContext)
    {
        var links = new List<LinkInfo>();

        if (list.HasPreviousPage)
        {
            links.Add(CreateLink(HttpContext, list.PreviousPageNumber,
                       list.PageSize, "previousPage", "GET"));
        }

        links.Add(CreateLink(HttpContext, list.PageNumber,
                       list.PageSize, "self", "GET"));

        if (list.HasNextPage)
        {
            links.Add(CreateLink(HttpContext, list.NextPageNumber,
                       list.PageSize, "nextPage", "GET"));
        }

        return links;
    }

    private LinkInfo CreateLink(
                                HttpContext HttpContext,
                                int pageNumber,
                                int pageSize,
                                string rel,
                                string method)
    {
        return new LinkInfo
        {
            Href = HttpContext.Request.Path + $"?PageNumber={pageNumber}&PageSize={pageSize}",
            Rel = rel,
            Method = method
        };
    }
}
