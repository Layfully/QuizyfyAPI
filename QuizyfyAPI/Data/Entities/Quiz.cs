#pragma warning disable CA1852
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizyfyAPI.Data.Entities;

public  class Quiz
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Description { get; set; }

    public DateTime DateAdded { get; set; }
    
    public int? ImageId { get; set; }

    [ForeignKey(nameof(ImageId))]
    public Image? Image { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = [];
    
    public virtual ICollection<Like> Likes { get; set; } = [];
}