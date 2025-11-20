using System.ComponentModel.DataAnnotations;
using QuizyfyAPI.Data;

namespace QuizyfyAPI.Contracts.Responses;

/// <summary>
/// A user with username, password, role, token and full name.
/// </summary>
public record UserResponse
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
    /// TODO: Remove it as its not secure to send password over network
    [Required]
    public required string Password { get; init; }

    /// <summary>
    /// User role. (Can be either admin or user)
    /// </summary>
    [Required]
    public required string Role { get; init; }

    /// <summary>
    /// JWT Token used for authentication
    /// </summary>
    public string? JwtToken { get; init; }

    /// <summary>
    /// Token used for refreshing JWT token.
    /// </summary>
    public RefreshToken? RefreshToken { get; init; }
}