namespace QuizyfyAPI.Contracts.Responses;

/// <summary>
/// A question with text(actual question) and collection of choices. Used for displaying questions.
/// </summary>
public sealed record QuestionResponse
{
    /// <summary>
    /// Question Id.
    /// </summary>
    [Required]
    public int Id { get; init; }

    /// <summary>
    /// Question text.
    /// </summary>
    [Required]
    [MaxLength(70)]
    public required string Text { get; init; }

    /// <summary>
    /// Url to optional image for question.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Possible question answers.
    /// </summary>
    [Required]
    public required ICollection<ChoiceResponse> Choices { get; init; }
}