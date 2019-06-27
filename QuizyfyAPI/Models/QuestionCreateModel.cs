using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    public class QuestionCreateModel
    {
        [Required]
        public string Text { get; set; }
    }
}
