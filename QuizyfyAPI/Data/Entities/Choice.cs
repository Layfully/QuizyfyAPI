#pragma warning disable CA1852
namespace QuizyfyAPI.Data.Entities;

internal class Choice
{
    [Key]
    public int Id { get; set; }
    public int QuestionId { get; set; }
    [Required]
    [MaxLength(500)]
    public required string Text { get; set; }
    public bool IsRight { get; set; }
}