namespace QuizyfyAPI.Options;

public record RateLimitOptions
{
    public string RealIpHeader { get; init; } = "X-Real-IP";
    public List<string> IpWhitelist { get; init; } = [];
}