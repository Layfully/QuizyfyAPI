using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// A quiz with name and questions properties. Used for DTO.
    /// </summary>
    public class QuizCreateModel
    {
        /// <summary>
        /// Quiz name.
        /// </summary>
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        /// <summary>
        /// Collection of questions which belongs to quiz.
        /// </summary>
        public ICollection<QuestionModel> Questions { get; set; }
    }
}
