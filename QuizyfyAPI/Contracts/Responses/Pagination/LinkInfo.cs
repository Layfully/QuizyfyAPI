using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses.Pagination;

public record LinkInfo
{
    [Required]
    public required string Href { get; init; }

    [Required]
    public required string Rel { get; init; }

    [Required]
    public required string Method { get; init; }
}
