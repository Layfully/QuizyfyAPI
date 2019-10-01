using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// A choice with text(actual answer) and isRight bool. Used for displaying questions and DTO.
    /// </summary>
    public class VerifyEmailRequest
    {
        /// <summary>
        /// Token.
        /// </summary>
        [Required]
        public string Token { get; set; }
    }
}
