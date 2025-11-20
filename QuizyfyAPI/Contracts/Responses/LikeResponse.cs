using System.ComponentModel.DataAnnotations;

/// <summary>
/// A like of quiz which belongs to some user.
/// </summary>
public record LikeResponse
{
    /// <summary>
    /// This id specifies to which quiz like belongs to.
    /// </summary>
    [Required]
    public required int QuizId { get; init; }

    /// <summary>
    /// This id specifies to which user liked quiz.
    /// </summary>
    [Required]
    public required int UserId { get; init; }
}