namespace QuizyfyAPI.Domain;

internal record BasicResult
{
    public bool Success { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];
}
