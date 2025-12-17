namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A choice with text(actual answer) and isRight bool. Used for displaying questions and DTO.
/// </summary>
internal sealed record ChoiceUpdateRequest
{
    /// <summary>
    /// Choice text (answer).
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Bool, which defines whether this answer is right or not.
    /// </summary>
    public bool? IsRight { get; init; }
}