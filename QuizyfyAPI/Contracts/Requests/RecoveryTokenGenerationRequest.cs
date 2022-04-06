using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;
/// <summary>
/// Request used to provide data for recovery password token generation.
/// </summary>
public class RecoveryTokenGenerationRequest
{
    /// <summary>
    /// An email adress to which generated recovery token will be sent.
    /// </summary>
    [EmailAddress]
    public string Email { get; set; } = null!;
}
