using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// Request used to provide data for recovery password token generation.
    /// </summary>
    public class RecoveryTokenGenerationRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
