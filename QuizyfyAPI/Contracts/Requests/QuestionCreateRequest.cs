using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// Question with text(actual question) only. Used for DTO.
/// </summary>
public record QuestionCreateRequest
{
    /// <summary>
    /// Question text.
    /// </summary>
    [Required]
    [MaxLength(70)]
    public required string Text { get; init; }

    public int? ImageId { get; init; } 

    [Required]
    public required ICollection<ChoiceCreateRequest> Choices { get; init; }
}