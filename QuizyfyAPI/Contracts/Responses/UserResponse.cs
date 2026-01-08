namespace QuizyfyAPI.Contracts.Responses;

public sealed record UserResponse
{
    public int Id { get; init; }
    
    public string? FirstName { get; init; }
    
    public string? LastName { get; init; }

    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Email { get; init; }
    
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
    public string? RefreshToken { get; init; }
}