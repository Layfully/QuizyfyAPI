using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Options;
public class AppOptions
{
    public bool ReturnHttpNotAcceptable { get; set; } = true;

    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public string ServerPath { get; set; } = null!;
}
