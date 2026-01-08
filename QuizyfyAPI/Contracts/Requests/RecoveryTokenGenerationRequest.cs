namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// Request used to provide data for recovery password token generation.
/// </summary>
public sealed record RecoveryTokenGenerationRequest
{
    [EmailAddress]
    [Required]
    public required string Email { get; init; }
}
