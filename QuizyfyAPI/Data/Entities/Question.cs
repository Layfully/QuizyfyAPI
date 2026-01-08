#pragma warning disable CA1852
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizyfyAPI.Data.Entities;

public  class Question
{
    [Key] 
    public int Id { get; set; }
    public int QuizId { get; set; }
    [Required]
    [MaxLength(500)]
    public required string Text { get; set; }
    public int? ImageId { get; set; }
    [ForeignKey(nameof(ImageId))] public Image? Image { get; set; }
    public virtual ICollection<Choice> Choices { get; set; } = [];
}
