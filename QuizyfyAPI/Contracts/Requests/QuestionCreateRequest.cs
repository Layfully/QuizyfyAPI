using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// Question with text(actual question) only. Used for DTO.
    /// </summary>
    public class QuestionCreateRequest
    {
        /// <summary>
        /// Question text.
        /// </summary>
        [MaxLength(70)]
        public string Text { get; set; } = null!;

        /// <summary>
        /// Id of an image which gives additional information about the question.
        /// </summary>
        public int? ImageId { get; set; } 

        /// <summary>
        /// List of possible choices to the question.
        /// </summary>
        public virtual ICollection<ChoiceCreateRequest> Choices { get; } = new List<ChoiceCreateRequest>();
    }
}
