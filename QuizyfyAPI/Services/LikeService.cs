﻿using AutoMapper;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;

namespace QuizyfyAPI.Services;
public class LikeService : ILikeService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IMapper _mapper;

    public LikeService(IQuizRepository quizRepository, ILikeRepository likeRepository, IMapper mapper)
    {
        _quizRepository = quizRepository;
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public async Task<ObjectResult<LikeResponse>> Like(int quizId, int userId)
    {
        var quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz == null)
        {
            return new ObjectResult<LikeResponse> { Errors = new[] { "Quiz with given id was not found" } };
        }

        Like like;

        if ((like = await _likeRepository.GetLike(quizId, userId)) != null)
        {
            return new ObjectResult<LikeResponse> { Found = true, Success = true, Object = _mapper.Map<LikeResponse>(like) };
        }

        var likeModel = new LikeResponse
        {
            QuizId = quizId,
            UserId = userId
        };

        like = _mapper.Map<Like>(likeModel);

        _likeRepository.Add(like);

        if (await _likeRepository.SaveChangesAsync())
        {
            return new ObjectResult<LikeResponse> { Found = true, Success = true, Object = likeModel };
        }

        return new ObjectResult<LikeResponse> { Found = true, Errors = new[] { "No rows were affected" } };
    }

    public async Task<DetailedResult> Delete(int quizId, int userId)
    {
        var quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz == null)
        {
            return new DetailedResult { Errors = new[] { "Quiz with given id was not found!" } };
        }

        var like = await _likeRepository.GetLike(quizId, userId);

        if (like == null)
        {
            return new DetailedResult { Errors = new[] { "Like for this quiz was not found!" } };
        }

        _likeRepository.Delete(like);

        if (await _likeRepository.SaveChangesAsync())
        {
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }
}
