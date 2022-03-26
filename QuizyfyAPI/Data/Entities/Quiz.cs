using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data;
public class Quiz
{
    [Key]
    public int Id { get; set; }
    public Image Image { get;set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.MinValue;
    public virtual ICollection<Question> Questions { get; set; }
    public virtual ICollection<Like> Likes { get; set; }
}
