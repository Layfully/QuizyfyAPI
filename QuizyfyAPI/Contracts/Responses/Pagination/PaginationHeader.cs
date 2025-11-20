using System.Text.Json;

namespace QuizyfyAPI.Contracts.Responses.Pagination;

public record PagingHeader(int TotalItems, int PageNumber, int PageSize, int TotalPages)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ToJson() => JsonSerializer.Serialize(this, _jsonOptions);
}