namespace QuizyfyAPI.Options;

internal sealed record AppOptions
{
    public string ConnectionString { get; init; } = string.Empty;
    public string ServerPath { get; init; } = string.Empty;
    public bool ReturnHttpNotAcceptable { get; init; }
    
    public override string ToString()
    {
        return $"{{ ReturnHttpNotAcceptable = {ReturnHttpNotAcceptable}, ServerPath = {ServerPath}, ConnectionString = ***REDACTED*** }}";
    }
}