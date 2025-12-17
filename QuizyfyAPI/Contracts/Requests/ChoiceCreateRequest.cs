namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A choice with text(actual answer) and isRight bool. Used for displaying questions and DTO.
/// </summary>
internal sealed record ChoiceCreateRequest
{
    /// <summary>
    /// Choice text (answer).
    /// </summary>
    [Required]
    public required string Text { get; init; }

    /// <summary>
    /// Bool, which defines whether this answer is right or not.
    /// </summary>
    [Required]
    public required bool IsRight { get; init; }
}