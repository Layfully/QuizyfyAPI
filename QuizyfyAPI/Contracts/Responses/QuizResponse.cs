using QuizyfyAPI.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// A quiz with name , date of addition and questions properties. Used for displaying quizzes.
    /// </summary>
    public class QuizResponse
    {
        /// <summary>
        /// Quiz id.
        /// </summary>
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// Quiz name.
        /// </summary>
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        /// <summary>
        /// Quiz description.
        /// </summary>
        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }
        /// <summary>
        /// Date of addition to database.
        /// </summary>
        [Required]
        public string DateAdded { get; set; }
        /// <summary>
        /// Collection of questions which belongs to quiz.
        /// </summary>
        public virtual ICollection<Question> Questions {get;set;}
    }
}
