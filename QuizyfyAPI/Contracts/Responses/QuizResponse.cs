namespace QuizyfyAPI.Contracts.Responses;
/// <summary>
/// A quiz with name, date of addition and questions properties. Used for displaying quizzes.
/// </summary>
internal sealed record QuizResponse
{
    /// <summary>
    /// Quiz id.
    /// </summary>
    [Required]
    public required int Id { get; init; }

    /// <summary>
    /// Quiz name.
    /// </summary>
    [Required]
    [MaxLength(70)]
    public required string Name { get; init; }

    /// <summary>
    /// Quiz description.
    /// </summary>
    [Required]
    public required string Description { get; init; }

    public string? ImageUrl { get; init; }

    /// <summary>
    /// Date of addition to a database.
    /// </summary>
    [Required]
    public required DateTime DateAdded { get; init; }

    /// <summary>
    /// Collection of questions which belongs to quiz.
    /// </summary>
    [Required]
    public required ICollection<QuestionResponse> Questions { get; init; }
}
