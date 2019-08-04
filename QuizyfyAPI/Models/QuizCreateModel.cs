using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI;
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
    [ModelBinder(typeof(JsonWithFilesFormDataModelBinder), Name = "json")]
    public class QuizCreateModel
    {
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
        public IFormFile Image { get; set; }
        /// <summary>
        /// Collection of questions which belongs to quiz.
        /// </summary>
        public ICollection<QuestionCreateModel> Questions { get; set; }
    }
}
