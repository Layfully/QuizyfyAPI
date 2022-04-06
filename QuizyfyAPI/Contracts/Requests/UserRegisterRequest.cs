using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;
/// <summary>
/// A user with username, password, role and full name. Used for creating user.
/// </summary>
public class UserRegisterRequest
{
    /// <summary>
    /// First name of the user owner.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the user owner.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User name.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// User role. (Can be either admin or user)
    /// </summary>
    public string Role { get; set; } = null!;

    /// <summary>
    /// Token used for checking if request was made by bot or real person.
    /// </summary>
    public string RecaptchaToken { get; set; } = null!;

    /// <summary>
    /// Email address on which we will send confirmation message.
    /// </summary>
    [EmailAddress]
    public string Email { get; set; } = null!;
}
