using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;
/// <summary>
/// A quiz with name and questions properties. Used for DTO.
/// </summary>
public class QuizCreateRequest
{
    /// <summary>
    /// Quiz name.
    /// </summary>
    [MaxLength(70)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Quiz description.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Quiz image url which we get when we upload image.
    /// </summary>
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// Collection of questions which belongs to quiz.
    /// </summary>
    public virtual ICollection<QuestionCreateRequest> Questions { get; } = new List<QuestionCreateRequest>();
}
