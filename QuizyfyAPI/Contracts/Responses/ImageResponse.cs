using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses;
/// <summary>
/// Image with id from database and url to access it.
/// </summary>
public record ImageResponse
{
    /// <summary>
    /// Image id.
    /// </summary>
    [Required]
    public required int Id { get; init; }

    /// <summary>
    /// URL to image resource on server.
    /// </summary>
    [Required]
    public required string ImageUrl { get; init; }
}