namespace QuizyfyAPI.Contracts.Responses.Pagination;

internal sealed record PagingParams
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 5;
}
