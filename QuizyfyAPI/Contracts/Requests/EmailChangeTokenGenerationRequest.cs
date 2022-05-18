using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    public class EmailChangeTokenGenerationRequest
    {
        [EmailAddress]
        public string NewEmail { get; set; } = null!;
    }
}
