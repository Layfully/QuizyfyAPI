using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Hybrid;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Mappers;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Services;

internal sealed class ChoiceService : IChoiceService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IChoiceRepository _choiceRepository;
    private readonly HybridCache _hybridCache;
    private readonly IOutputCacheStore _outputCache;

    public ChoiceService(
        IQuestionRepository questionRepository,
        IChoiceRepository choiceRepository,
        HybridCache hybridCache,
        IOutputCacheStore outputCache)
    {
        _questionRepository = questionRepository;
        _choiceRepository = choiceRepository;
        _hybridCache = hybridCache;
        _outputCache = outputCache;
    }

    public async Task<ObjectResult<ChoiceResponse[]>> GetAll(int quizId, int questionId)
    {
        string cacheKey = $"Choices_Question_{questionId}";

        ICollection<Choice>? choices = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                Question? question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices: true);
                return question?.Choices;
            },
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(10) },
            tags: [$"Quiz:{quizId}", $"Question:{questionId}"]
        );

        if (choices is null)
        {
            return new ObjectResult<ChoiceResponse[]> { Errors = ["Failed to find the question"] };
        }
        
        if (choices.Count == 0)
        {
            return new ObjectResult<ChoiceResponse[]> { Found = true, Errors = ["This question doesn't have any choices"] };
        }

        return new ObjectResult<ChoiceResponse[]> 
        { 
            Object = choices.Select(c => c.ToResponse()).ToArray(),
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
        
        Choice? choice = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async _ => await _choiceRepository.GetChoice(questionId, choiceId),
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quizId}", $"Question:{questionId}", $"Choice:{choiceId}"]
        );

        if (choice is null)
        {
            return new ObjectResult<ChoiceResponse> { Errors = ["Failed to find the choice"] };
        }

        return new ObjectResult<ChoiceResponse> 
        { 
            Object = choice.ToResponse(), 
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

        Choice choice = request.ToEntity();
        choice.QuestionId = question.Id;
        
        _choiceRepository.Add(choice);

        if (!await _choiceRepository.SaveChangesAsync())
        {
            return new ObjectResult<ChoiceResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
        }
        
        await _hybridCache.RemoveByTagAsync([$"Question:{questionId}", $"Quiz:{quizId}"]);
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);

        await _hybridCache.SetAsync(
            $"Choice_{choice.Id}", 
            choice, 
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags:  [$"Quiz:{quizId}", $"Question:{questionId}", $"Choice:{choice.Id}"]
        );
            
        return new ObjectResult<ChoiceResponse> 
        { 
            Object = choice.ToResponse(), 
            Found = true, 
            Success = true 
        };
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

        choice.UpdateFrom(request);
        
        _choiceRepository.Update(choice);

        if (!await _choiceRepository.SaveChangesAsync())
        {
            return new ObjectResult<ChoiceResponse> { Found = true, Errors = ["Action didn't affect any rows"] };
        }
        
        await _hybridCache.RemoveByTagAsync([$"Choice:{choiceId}", $"Question:{questionId}", $"Quiz:{quizId}"]);
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
            
        await _hybridCache.SetAsync(
            $"Choice_{choice.Id}", 
            choice, 
            options: new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(5) },
            tags: [$"Quiz:{quizId}", $"Question:{questionId}", $"Choice:{choiceId}"]
        );

        return new ObjectResult<ChoiceResponse> 
        { 
            Object = choice.ToResponse(), 
            Found = true, 
            Success = true 
        };
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

        if (!await _choiceRepository.SaveChangesAsync())
        {
            return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
        }
        
        await _hybridCache.RemoveByTagAsync([$"Choice:{choiceId}", $"Question:{questionId}", $"Quiz:{quizId}"]);
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
        
        return new DetailedResult { Found = true, Success = true };
    }
}
