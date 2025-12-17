namespace QuizyfyAPI.Services.Interfaces;

internal interface ILikeService
{
    Task<ObjectResult<LikeResponse>> Like(int quizId, int userId);

    Task<DetailedResult> Delete(int quizId, int userId);
}
