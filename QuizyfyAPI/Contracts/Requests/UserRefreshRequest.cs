namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// Request used for refreshing user JWT token.
/// </summary>
public sealed record UserRefreshRequest
{
    /// <summary>
    /// Refresh token which lasts for a long time.
    /// </summary>
    [Required]
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Already expired previous JWT token which needs to be refreshed.
    /// </summary>
    [Required]
    public required string JwtToken { get; init; }
}
