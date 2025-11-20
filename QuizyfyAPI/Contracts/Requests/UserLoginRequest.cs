using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A user credentials. Used for authentication
/// </summary>
public record UserLoginRequest
{
    /// <summary>
    /// User name.
    /// </summary>
    [Required]
    public required string Username { get; init; }

    /// <summary>
    /// User password.
    /// </summary>
    [Required]
    public required string Password { get; init; }

    /// <summary>
    /// Token for checking if request is made by bot or real person.
    /// </summary>
    [Required]
    public required string RecaptchaToken { get; init; }
}
