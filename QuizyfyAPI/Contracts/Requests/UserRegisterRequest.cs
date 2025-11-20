using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A user with username, password, role and full name. Used for creating user.
/// </summary>
public record UserRegisterRequest
{
    /// <summary>
    /// First name of the user owner.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name of the user owner.
    /// </summary>
    public string? LastName { get; init; }

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
    /// User role. (Can be either admin or user)
    /// </summary>
    [Required]
    public required string Role { get; init; }

    /// <summary>
    /// Token used for checking if request was made by bot or real person.
    /// </summary>
    [Required]
    public required string RecaptchaToken { get; init; }

    /// <summary>
    /// Email address on which we will send confirmation message.
    /// </summary>
    [EmailAddress]
    [Required]
    public required string Email { get; init; }
}