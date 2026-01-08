#pragma warning disable CA1852
namespace QuizyfyAPI.Data.Entities;

public  class Image
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public required string ImageUrl { get; set; }
}