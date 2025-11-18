namespace QuizyfyAPI.Options;

public record AppOptions
{
    public required string ConnectionString { get; init; }
    public required string ServerPath { get; init; }
    public bool ReturnHttpNotAcceptable { get; init; }
    
    public override string ToString()
    {
        return $"{{ ReturnHttpNotAcceptable = {ReturnHttpNotAcceptable}, ServerPath = {ServerPath}, ConnectionString = ***REDACTED*** }}";
    }
}