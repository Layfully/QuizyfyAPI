using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;
/// <summary>
/// Question with text(actual question) only. Used for DTO.
/// </summary>
public class QuestionUpdateRequest
{
    /// <summary>
    /// Question text.
    /// </summary>
    [MaxLength(70)]
    public string? Text { get; set; }

    /// <summary>
    /// Id of an image which gives additional information about the question.
    /// </summary>
    public int? ImageId { get; set; }
}
