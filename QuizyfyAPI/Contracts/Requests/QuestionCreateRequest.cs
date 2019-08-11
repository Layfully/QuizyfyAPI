using QuizyfyAPI.Contracts.Responses;
using System.Collections.Generic;
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
        [Required]
        [MaxLength(70)]
        public string Text { get; set; }
        public virtual ICollection<ChoiceCreateRequest> Choices { get; set; }
    }
}
