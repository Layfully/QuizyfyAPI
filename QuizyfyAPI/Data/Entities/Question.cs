﻿using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data;
public class Question
{
    [Key]
    public int Id { get; set; }
    public Image Image { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; }
    public virtual ICollection<Choice> Choices { get; set; }
}
