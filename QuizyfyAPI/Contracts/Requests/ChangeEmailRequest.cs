using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    public class ChangeEmailRequest
    {
        [EmailAddress]
        public string NewEmail { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
