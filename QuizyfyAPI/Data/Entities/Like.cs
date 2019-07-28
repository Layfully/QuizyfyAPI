using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class Like
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
    }
}
