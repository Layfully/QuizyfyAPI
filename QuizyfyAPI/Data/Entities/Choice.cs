using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data;
public class Choice
{
    [Key]
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; }
    public bool IsRight { get; set; }
}
