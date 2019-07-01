using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// A question with text(actual question) and collection of choices. Used for displaying questions.
    /// </summary>
    public class QuestionModel
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
        public ICollection<ChoiceModel> Choices { get; set; }
    }
}
