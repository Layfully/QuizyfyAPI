namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// User credentials. Used for authentication
/// </summary>
internal sealed record UserLoginRequest : IRecaptchaRequest
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
    /// Token for checking if a request is made by a bot or real person.
    /// </summary>
    [Required]
    public required string RecaptchaToken { get; init; }
}
