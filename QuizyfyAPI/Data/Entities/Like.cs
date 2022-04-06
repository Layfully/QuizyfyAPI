namespace QuizyfyAPI.Data;
/// <summary>
/// A Like model, which is a representation of row from Likes table.
/// </summary>
public class Like
{
    /// <summary>
    /// Id of a Like.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// A Quiz to which like belongs to.
    /// </summary>
    public int QuizId { get; set; }
    /// <summary>
    /// A User to which like belongs to.
    /// </summary>
    public int UserId { get; set; }
}
