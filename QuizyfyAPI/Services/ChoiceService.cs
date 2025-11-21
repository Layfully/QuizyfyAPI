using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;

namespace QuizyfyAPI.Services;
public class ChoiceService : IChoiceService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IChoiceRepository _choiceRepository;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<ChoiceService> _logger;
    
    public ChoiceService(
        IQuestionRepository questionRepository,
        IChoiceRepository choiceRepository,
        IMemoryCache cache,
        IMapper mapper,
        ILogger<ChoiceService> logger)
    {
        _questionRepository = questionRepository;
        _choiceRepository = choiceRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ObjectResult<ChoiceResponse[]>> GetAll(int quizId, int questionId)
    {
        string cacheKey = $"Choices_Question_{questionId}";

        if (!_cache.TryGetValue(cacheKey, out ICollection<Choice>? choices))
        {
            Question? question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices: true);
            
            if (question is null)
            {
                return new ObjectResult<ChoiceResponse[]> { Errors = ["Failed to find the question"] };
            }

            choices = question.Choices;
            _cache.Set(cacheKey, choices, TimeSpan.FromMinutes(10));
        }

        if (choices is null || choices.Count == 0)
        {
            return new ObjectResult<ChoiceResponse[]> { Found = true, Errors = ["This question doesn't have any choices"] };
        }

        return new ObjectResult<ChoiceResponse[]> 
        { 
            Object = _mapper.Map<ChoiceResponse[]>(choices), 
            Found = true, 
            Success = true 
        };    
    }

    public async Task<ObjectResult<ChoiceResponse>> Get(int quizId, int questionId, int choiceId)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId);
        if (question is null)
        {
            return new ObjectResult<ChoiceResponse> { Errors = ["Failed to find the question"] };
        }
        
        string cacheKey = $"Choice_{choiceId}";
        
        Choice? choice = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _choiceRepository.GetChoice(questionId, choiceId);
        });

        if (choice is null)
        {
            return new ObjectResult<ChoiceResponse> { Errors = ["Failed to find the choice"] };
        }

        return new ObjectResult<ChoiceResponse> 
        { 
            Object = _mapper.Map<ChoiceResponse>(choice), 
            Found = true, 
            Success = true 
        };
    }

    public async Task<ObjectResult<ChoiceResponse>> Create(int quizId, int questionId, ChoiceCreateRequest request)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId);
        
        if (question is null)
        {
            return new ObjectResult<ChoiceResponse> { Errors = ["Didn't find the question"] };
        }

        Choice choice = _mapper.Map<Choice>(request);
        choice.QuestionId = question.Id;
        
        _choiceRepository.Add(choice);
        
        if (await _choiceRepository.SaveChangesAsync())
        {
            _cache.Set($"Choice_{choice.Id}", choice, TimeSpan.FromMinutes(5));
            _cache.Remove($"Choices_Question_{questionId}");

            return new ObjectResult<ChoiceResponse> 
            { 
                Object = _mapper.Map<ChoiceResponse>(choice), 
                Found = true, 
                Success = true 
            };
        }

        return new ObjectResult<ChoiceResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<ObjectResult<ChoiceResponse>> Update(int quizId, int questionId, int choiceId, ChoiceUpdateRequest request)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId);
        if (question is null)
        {
            return new ObjectResult<ChoiceResponse> { Errors = ["Failed to find the question"] };
        }
        
        Choice? choice = await _choiceRepository.GetChoice(questionId, choiceId);
        if (choice is null)
        {
            return new ObjectResult<ChoiceResponse> { Errors = ["Failed to find the choice to update"] };
        }

        _mapper.Map(request, choice);
        
        _choiceRepository.Update(choice);

        if (await _choiceRepository.SaveChangesAsync())
        {
            _cache.Set($"Choice_{choice.Id}", choice, TimeSpan.FromMinutes(5));
            _cache.Remove($"Choices_Question_{questionId}");

            return new ObjectResult<ChoiceResponse> 
            { 
                Object = _mapper.Map<ChoiceResponse>(choice), 
                Found = true, 
                Success = true 
            };
        }

        return new ObjectResult<ChoiceResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<DetailedResult> Delete(int quizId, int questionId, int choiceId)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId);
        if (question is null)
        {
            return new DetailedResult { Errors = ["Failed to find the question"] };
        }
        
        Choice? choice = await _choiceRepository.GetChoice(questionId, choiceId);
        if (choice is null)
        {
            return new DetailedResult { Errors = ["Failed to find the choice to delete"] };
        }
        
        _choiceRepository.Delete(choice);

        if (await _choiceRepository.SaveChangesAsync())
        {
            _cache.Remove($"Choice_{choiceId}");
            _cache.Remove($"Choices_Question_{questionId}");
            return new DetailedResult { Found = true, Success = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }
}
