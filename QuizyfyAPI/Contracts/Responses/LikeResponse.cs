namespace QuizyfyAPI.Contracts.Responses;
/// <summary>
/// A like of quiz which belongs to some user.
/// </summary>
public class LikeResponse
{
    /// <summary>
    /// This id specifies to which quiz like belongs to.
    /// </summary>
    public int QuizId { get; set; }

    /// <summary>
    /// This id specifies to which user liked quiz.
    /// </summary>
    public int UserId { get; set; }
}
