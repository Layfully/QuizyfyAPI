﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class Quiz
    {
        public int Id { get; set; }
        public Image Image { get;set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.MinValue;
        public ICollection<Question> Questions { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
