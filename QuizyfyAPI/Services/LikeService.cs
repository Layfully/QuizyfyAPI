using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Hybrid;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Mappers;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Services;

internal sealed class LikeService : ILikeService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly HybridCache _hybridCache;
    private readonly IOutputCacheStore _outputCache;

    public LikeService(
        IQuizRepository quizRepository, 
        ILikeRepository likeRepository,
        HybridCache hybridCache,
        IOutputCacheStore outputCache)
    {
        _quizRepository = quizRepository;
        _likeRepository = likeRepository;
        _hybridCache = hybridCache;
        _outputCache = outputCache;
    }

    public async Task<ObjectResult<LikeResponse>> Like(int quizId, int userId)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz is null)
        {
            return new ObjectResult<LikeResponse> { Errors = ["Quiz with given id was not found"] };
        }

        Like? existingLike = await _likeRepository.GetLike(quizId, userId);

        if (existingLike is not null)
        {
            return new ObjectResult<LikeResponse> 
            { 
                Found = true, 
                Success = true, 
                Object = existingLike.ToResponse() 
            };
        }
        
        Like like = new()
        {
            QuizId = quizId,
            UserId = userId
        };

        _likeRepository.Add(like);

        if (!await _likeRepository.SaveChangesAsync())
        {
            return new ObjectResult<LikeResponse> { Found = true, Errors = ["No rows were affected"] };
        }
        
        await _hybridCache.RemoveByTagAsync($"Quiz:{quizId}");
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
            
        return new ObjectResult<LikeResponse> 
        { 
            Found = true, 
            Success = true, 
            Object = like.ToResponse() 
        };

    }

    public async Task<DetailedResult> Delete(int quizId, int userId)
    {
        Quiz? quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz is null)
        {
            return new DetailedResult { Errors = ["Quiz with given id was not found!"] };
        }

        Like? like = await _likeRepository.GetLike(quizId, userId);

        if (like is null)
        {
            return new DetailedResult { Errors = ["Like for this quiz was not found!"] };
        }

        _likeRepository.Delete(like);

        if (!await _likeRepository.SaveChangesAsync())
        {
            return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
        }
        
        await _hybridCache.RemoveByTagAsync($"Quiz:{quizId}");
        await _outputCache.EvictByTagAsync("quizzes", CancellationToken.None);
        
        return new DetailedResult { Success = true, Found = true };
    }
}
