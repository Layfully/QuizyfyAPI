using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// Question with text(actual question) only. Used for DTO.
    /// </summary>
    public class QuestionCreateModel
    {
        /// <summary>
        /// Question text.
        /// </summary>
        [Required]
        [MaxLength(70)]
        public string Text { get; set; }
    }
}
