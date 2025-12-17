#pragma warning disable CA1852
namespace QuizyfyAPI.Data.Entities;

internal class Like
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }
    public int UserId { get; set; }
}
