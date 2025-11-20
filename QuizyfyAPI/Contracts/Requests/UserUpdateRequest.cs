using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;
/// <summary>
/// A user with username, password, role and full name. Used for creating user.
/// </summary>

/// <summary>
/// A user with username, password, role and full name. Used for creating user.
/// </summary>
public record UserUpdateRequest
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
    public string? Username { get; init; }

    /// <summary>
    /// User password.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// User role. (Can be either admin or user)
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// User email. (Must be unique)
    /// </summary>
    [EmailAddress]
    public string? Email { get; init; }
}
