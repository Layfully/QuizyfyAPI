namespace QuizyfyAPI.Data;
/// <summary>
/// A Quiz model, which is a representation of row from Quizzes table.
/// </summary>
public class Quiz
{
    /// <summary>
    /// Id of a Quiz.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// An image which is thumbnail of a quiz.
    /// </summary>
    public Image Image { get; set; } = null!;
    /// <summary>
    /// Name of a quiz.
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// Description of a Quiz.
    /// </summary>
    public string Description { get; set; } = null!;
    /// <summary>
    /// Date of creation of a quiz.
    /// </summary>
    public DateTime DateAdded { get; set; } = DateTime.MinValue;
    /// <summary>
    /// List of questions, which belong to quiz.
    /// </summary>
    public virtual ICollection<Question> Questions { get; } = new List<Question>();
    /// <summary>
    /// List of likes, which belong to quiz.
    /// </summary>
    public virtual ICollection<Like> Likes { get; } = new List<Like>();
}
