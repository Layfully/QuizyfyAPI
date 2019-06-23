using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class Choice
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsRight { get; set; }
    }
}
