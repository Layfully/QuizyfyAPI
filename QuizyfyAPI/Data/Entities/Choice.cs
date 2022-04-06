namespace QuizyfyAPI.Data;
/// <summary>
/// A Choice model, which is a representation of row from Choices table.
/// </summary>
public class Choice
{    
    /// <summary>
    /// Id of a Choice.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Id of a question a choice belongs to.
    /// </summary>
    public int QuestionId { get; set; }
    /// <summary>
    /// Text description of a choice.
    /// </summary>
    public string Text { get; set; } = null!;
    /// <summary>
    /// A flag, which specifies whether the choice is true or false.
    /// </summary>
    public bool IsRight { get; set; }
}
