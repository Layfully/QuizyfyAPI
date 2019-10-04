using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// A question with text(actual question) and collection of choices. Used for displaying questions.
    /// </summary>
    public class QuestionResponse
    {
        /// <summary>
        /// Question text.
        /// </summary>
        [Required]
        [MaxLength(70)]
        public string Text { get; set; }

        /// <summary>
        /// Possible question answers.
        /// </summary>
        public ICollection<QuestionResponse> Choices { get; set; }
    }
}
