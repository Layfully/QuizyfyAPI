using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Hybrid;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Mappers;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Services;

internal sealed class QuestionService : IQuestionService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IOutputCacheStore _outputCache;
    private readonly HybridCache _hybridCache;

    public QuestionService(
        IQuizRepository quizRepository, 
        IQuestionRepository questionRepository, 
        IImageRepository imageRepository,
        IOutputCacheStore outputCache,
        HybridCache hybridCache)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _imageRepository = imageRepository;
        _outputCache = outputCache;
        _hybridCache = hybridCache;
    }

    public async Task<ObjectResult<QuestionResponse[]>> GetAll(int quizId, bool includeChoices)
    {
        string cacheKey = $"Questions_Quiz_{quizId}_{includeChoices}";

        Question[] questions = await _hybridCache.GetOrCreateAsync(
            cacheKey, 
            async _ => await _questionRepository.GetQuestions(quizId, includeChoices),
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(10) },
            tags: [$"Quiz:{quizId}"] 
        );

        if (questions.Length == 0)
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
            Object = questions.Select(q => q.ToResponse()).ToArray() 
        };
    }

    public async Task<ObjectResult<QuestionResponse>> Get(int quizId, int questionId, bool includeChoices = false)
    {
        string cacheKey = $"Question_{questionId}_{includeChoices}";
        
        Question? question = await _hybridCache.GetOrCreateAsync(
            cacheKey, 
            async _ => await _questionRepository.GetQuestion(quizId, questionId, includeChoices),
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quizId}", $"Question:{questionId}"]
        );

        if (question is null)
        {
            return new ObjectResult<QuestionResponse> { Errors = ["Couldn't find this question"] };
        }

        return new ObjectResult<QuestionResponse> 
        { 
            Object = question.ToResponse(),
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

        Question question = request.ToEntity();
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

        if (!await _questionRepository.SaveChangesAsync())
        {
            return new ObjectResult<QuestionResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
        }
        
        await _hybridCache.RemoveByTagAsync($"Quiz:{quizId}");
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
        
        await _hybridCache.SetAsync(
            $"Question_{question.Id}_False", 
            question, 
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quizId}", $"Question:{question.Id}"]
        );

        return new ObjectResult<QuestionResponse> 
        { 
            Success = true, 
            Found = true, 
            Object = question.ToResponse()
        };
    }

    public async Task<ObjectResult<QuestionResponse>> Update(int quizId, int questionId, QuestionUpdateRequest request)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices: true);

        if (question is null)
        {
            return new ObjectResult<QuestionResponse> { Errors = ["Couldn't find question"] };
        }

        question.UpdateFrom(request);
        
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

        if (!await _questionRepository.SaveChangesAsync())
        {
            return new ObjectResult<QuestionResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
        }

        await _hybridCache.RemoveByTagAsync([$"Question:{questionId}", $"Quiz:{quizId}"]);
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);

        await _hybridCache.SetAsync(
            $"Question_{question.Id}_False", 
            question, 
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quizId}", $"Question:{questionId}"]
        );
        
        return new ObjectResult<QuestionResponse> 
        { 
            Object = question.ToResponse(), 
            Found = true, 
            Success = true 
        };
    }

    public async Task<DetailedResult> Delete(int quizId, int questionId)
    {
        Question? question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices: false);
        
        if (question is null)
        {
            return new DetailedResult { Errors = ["Failed to find the question to delete"] };
        }

        _questionRepository.Delete(question);

        if (!await _questionRepository.SaveChangesAsync())
        {
            return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
        }

        await _hybridCache.RemoveByTagAsync([$"Question:{questionId}", $"Quiz:{quizId}"]);
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
            
        return new DetailedResult { Success = true, Found = true };
    }
}