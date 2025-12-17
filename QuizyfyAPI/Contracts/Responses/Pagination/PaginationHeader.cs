using System.Text.Json;

namespace QuizyfyAPI.Contracts.Responses.Pagination;

internal sealed record PagingHeader(int TotalItems, int PageNumber, int PageSize, int TotalPages)
{
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, AppJsonSerializerContext.Default.PagingHeader);
    }
}