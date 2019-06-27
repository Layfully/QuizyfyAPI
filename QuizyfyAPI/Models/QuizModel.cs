using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// A quiz with name , date of addition and questions properties.
    /// </summary>
    public class QuizModel
    {
        /// <summary>
        /// Quiz name.
        /// </summary>
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        /// <summary>
        /// Date of addition to database.
        /// </summary>
        [Required]
        public string DateAdded { get; set; }
        /// <summary>
        /// Collection of questions which belongs to quiz.
        /// </summary>
        public ICollection<QuestionModel> Questions {get;set;}
    }
}
