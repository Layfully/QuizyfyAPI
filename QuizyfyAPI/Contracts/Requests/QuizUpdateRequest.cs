using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A quiz with name and questions properties. Used for DTO.
/// </summary>
public record QuizUpdateRequest
{
    /// <summary>
    /// Quiz name.
    /// </summary>
    [MaxLength(70)]
    public string? Name { get; init; }

    /// <summary>
    /// Quiz description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Quiz image id which we get when we upload image.
    /// </summary>
    public int? ImageId { get; init; }
}