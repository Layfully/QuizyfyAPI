namespace QuizyfyAPI.Domain;

internal record DetailedResult : BasicResult
{
    public bool Found { get; init; }
}