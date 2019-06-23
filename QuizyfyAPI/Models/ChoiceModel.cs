using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    public class ChoiceModel
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public bool IsRight { get; set; }
    }
}
