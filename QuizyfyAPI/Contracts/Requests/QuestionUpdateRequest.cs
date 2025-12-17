namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// Question with text(actual question) only. Used for DTO.
/// </summary>
internal sealed record QuestionUpdateRequest
{
    /// <summary>
    /// Question text.
    /// </summary>
    [MaxLength(70)]
    public string? Text { get; init; }

    public int? ImageId { get; init; }
}