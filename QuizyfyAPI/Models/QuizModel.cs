using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    public class QuizModel
    {
        [Required]
        [StringLength(70)]
        public string Name { get; set; }
        [Required]
        public string DateAdded { get; set; }
        public ICollection<QuestionModel> Questions {get;set;}
    }
}
