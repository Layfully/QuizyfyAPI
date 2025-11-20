using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses;

/// <summary>
/// Error with status code and message.
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Int which tells us error status code.
    /// </summary>
    [Required]
    public required int StatusCode { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    [Required]
    public required string Message { get; init; }
}