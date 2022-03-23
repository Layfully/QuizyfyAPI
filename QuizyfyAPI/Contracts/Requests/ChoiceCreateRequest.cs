using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests;

/// <summary>
/// A choice with text(actual answer) and isRight bool. Used for displaying questions and DTO.
/// </summary>
public class ChoiceCreateRequest
{
    /// <summary>
    /// Choice text (answer).
    /// </summary>
    [Required]
    public string Text { get; set; }

    /// <summary>
    /// Bool which defines whether this answer is right or not.
    /// </summary>
    [Required]
    public bool IsRight { get; set; }
}
