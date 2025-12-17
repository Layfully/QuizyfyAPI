namespace QuizyfyAPI.Contracts.Responses;
/// <summary>
/// A like of quiz which belongs to some user.
/// </summary>
internal sealed record LikeResponse
{
    /// <summary>
    /// This id specifies to which quiz like belongs to.
    /// </summary>
    [Required]
    public required int QuizId { get; init; }

    /// <summary>
    /// This id specifies to which user liked the quiz.
    /// </summary>
    [Required]
    public required int UserId { get; init; }
}