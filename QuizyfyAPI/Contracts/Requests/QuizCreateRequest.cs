using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A quiz with name and questions properties. Used for DTO.
/// </summary>
public record QuizCreateRequest
{
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

    /// <summary>
    /// Quiz image url which we get when we upload image.
    /// </summary>
    [Required]
    public required string ImageUrl { get; init; }

    /// <summary>
    /// Collection of questions which belongs to quiz.
    /// </summary>
    [Required]
    public required ICollection<QuestionCreateRequest> Questions { get; init; }
}