using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses.Pagination;

public record QuizListResponse
{
    [Required]
    public required PagingHeader Paging { get; init; }

    [Required]
    public required List<LinkInfo> Links { get; init; }

    [Required]
    public required List<QuizResponse> Items { get; init; }
}