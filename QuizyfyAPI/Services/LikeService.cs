using AutoMapper;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;

namespace QuizyfyAPI.Services;
public class LikeService : ILikeService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IMapper _mapper;

    public LikeService(
        IQuizRepository quizRepository, 
        ILikeRepository likeRepository, 
        IMapper mapper)
    {
        _quizRepository = quizRepository;
        _likeRepository = likeRepository;
        _mapper = mapper;
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
                Object = _mapper.Map<LikeResponse>(existingLike) 
            };
        }
        
        Like like = new()
        {
            QuizId = quizId,
            UserId = userId
        };

        _likeRepository.Add(like);

        if (await _likeRepository.SaveChangesAsync())
        {
            return new ObjectResult<LikeResponse> 
            { 
                Found = true, 
                Success = true, 
                Object = _mapper.Map<LikeResponse>(like) 
            };
        }

        return new ObjectResult<LikeResponse> { Found = true, Errors = ["No rows were affected"] };
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

        if (await _likeRepository.SaveChangesAsync())
        {
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = ["Action didn't affect any rows"] };
    }
}
