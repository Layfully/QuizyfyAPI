using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data;
/// <summary>
/// A Question model, which is a representation of row from Questions table.
/// </summary>
public class Question
{
    public int Id { get; set; }
    /// <summary>
    /// An image which is additional information to question.
    /// </summary>
    public Image? Image { get; set; }
    /// <summary>
    /// A quiz a question belongs to.
    /// </summary>
    public int QuizId { get; set; }
    /// <summary>
    /// Text description of a question.
    /// </summary>
    public string Text { get; set; } = null!;
    /// <summary>
    /// List of possible choices for a question.
    /// </summary>
    public virtual ICollection<Choice> Choices { get; } = new List<Choice>();
}
