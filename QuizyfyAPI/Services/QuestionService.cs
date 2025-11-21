using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;

namespace QuizyfyAPI.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;

    public QuestionService(
        IQuizRepository quizRepository, 
        IQuestionRepository questionRepository, 
        IImageRepository imageRepository,
        IMemoryCache cache, 
        IMapper mapper)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _imageRepository = imageRepository;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<ObjectResult<QuestionResponse[]>> GetAll(int quizId, bool includeChoices)
    {
        string cacheKey = $"Questions_Quiz_{quizId}_{includeChoices}";

        ICollection<Question>? questions = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _questionRepository.GetQuestions(quizId, includeChoices);
        });

        if (questions is null || questions.Count == 0)
        {
            Quiz? quiz = await _quizRepository.GetQuiz(quizId);
            if (quiz is null)
            {
                return new ObjectResult<QuestionResponse[]> { Errors = ["Quiz with this id doesn't exist"] };
            }
            
            return new ObjectResult<QuestionResponse[]> { Found = true, Success = true, Object = [] };
        }
        
        return new ObjectResult<QuestionResponse[]> 
        { 
            Found = true, 
            Success = true, 
            Object = _mapper.Map<QuestionResponse[]>(questions) 
        };
    }

    public async Task<ObjectResult<QuestionResponse>> Get(int quizId, int questionId, bool includeChoices = false)
    {
        string cacheKey = $"Question_{questionId}_{includeChoices}";
        
        Question? question = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _questionRepository.GetQuestion(quizId, questionId, includeChoices);
        });

        if (question is null)
        {
            return new ObjectResult<QuestionResponse> { Errors = ["Couldn't find this question"] };
        }

        return new ObjectResult<QuestionResponse> 
        { 
            Object = _mapper.Map<QuestionResponse>(question), 
            Found = true, 
            Success = true 
        };
    }

    public async Task<ObjectResult<QuestionResponse>> Create(int quizId, QuestionCreateRequest request)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);
        if (quiz is null)
        {
            return new ObjectResult<QuestionResponse> { Errors = ["Couldn't find this quiz"] };
        }

        Question question = _mapper.Map<Question>(request);
        question.QuizId = quiz.Id;
        
        if (request.ImageId.HasValue && request.ImageId != 0)
        {
            Image? image = await _imageRepository.GetImage(request.ImageId.Value);
            if (image is null)
            {
                 return new ObjectResult<QuestionResponse> { Errors = ["Invalid ImageId provided"] };
            }
            question.Image = image;
        }

        _questionRepository.Add(question);
        
        if (await _questionRepository.SaveChangesAsync())
        {
            _cache.Set($"Question_{question.Id}_False", question, TimeSpan.FromMinutes(5));
            _cache.Remove($"Questions_Quiz_{quizId}_True");
            _cache.Remove($"Questions_Quiz_{quizId}_False");

            return new ObjectResult<QuestionResponse> 
            { 
                Success = true, 
                Found = true, 
                Object = _mapper.Map<QuestionResponse>(question) 
            };
        }
        
        return new ObjectResult<QuestionResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<ObjectResult<QuestionResponse>> Update(int quizId, int questionId, QuestionUpdateRequest request)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices: true);

        if (question is null)
        {
            return new ObjectResult<QuestionResponse> { Errors = ["Couldn't find question"] };
        }

        _mapper.Map(request, question);
        
        if (request.ImageId.HasValue && request.ImageId != 0)
        {
             Image? newImage = await _imageRepository.GetImage(request.ImageId.Value);
             if (newImage is null)
             {
                 return new ObjectResult<QuestionResponse> { Errors = ["Invalid ImageId provided"] };
             }
             question.Image = newImage;
        }
        
        _questionRepository.Update(question);

        if (await _questionRepository.SaveChangesAsync())
        {
            _cache.Remove($"Question_{questionId}_True");
            _cache.Remove($"Question_{questionId}_False");
            _cache.Remove($"Questions_Quiz_{quizId}_True");
            _cache.Remove($"Questions_Quiz_{quizId}_False");
            
            _cache.Set($"Question_{question.Id}_False", question, TimeSpan.FromMinutes(5));

            return new ObjectResult<QuestionResponse> 
            { 
                Object = _mapper.Map<QuestionResponse>(question), 
                Found = true, 
                Success = true 
            };
        }

        return new ObjectResult<QuestionResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
    }

    public async Task<DetailedResult> Delete(int quizId, int questionId)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices: false);
        
        if (question is null)
        {
            return new DetailedResult { Errors = ["Failed to find the question to delete"] };
        }

        _questionRepository.Delete(question);
        
        if (await _questionRepository.SaveChangesAsync())
        {
            _cache.Remove($"Question_{questionId}_True");
            _cache.Remove($"Question_{questionId}_False");
            _cache.Remove($"Questions_Quiz_{quizId}_True");
            _cache.Remove($"Questions_Quiz_{quizId}_False");
            
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }
}