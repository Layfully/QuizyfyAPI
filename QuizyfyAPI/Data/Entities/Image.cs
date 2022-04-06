namespace QuizyfyAPI.Data;
/// <summary>
/// An Image model, which is a representation of row from Images table.
/// </summary>
public class Image
{
    /// <summary>
    /// Id of an image.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// URL which specifies path to image on a server.
    /// </summary>
    public string ImageUrl { get; set; } = null!;
}
