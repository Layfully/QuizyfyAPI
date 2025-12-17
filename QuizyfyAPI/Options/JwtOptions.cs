namespace QuizyfyAPI.Options;

internal sealed class JwtOptions
{
    public string Secret { get; init; } = string.Empty;
    public bool ValidateIssuerSigningKey { get; init; } = true;
    public bool ValidateIssuer { get; init; } = true;
    public bool ValidateAudience { get; init; } = true;
    public bool ValidateLifetime { get; init; } = true;
    public bool RequireExpirationTime { get; init; } = true;
    public bool SaveToken { get; init; }
    public bool RequireHttpsMetadata { get; init; }
    
    public override string ToString()
    {
        return $$"""
                 {{nameof(JwtOptions)}}
                 {
                     Secret = ***REDACTED***,
                     ValidateIssuerSigningKey = {{ValidateIssuerSigningKey}},
                     ValidateIssuer = {{ValidateIssuer}},
                     ValidateAudience = {{ValidateAudience}},
                     ValidateLifetime = {{ValidateLifetime}},
                     RequireExpirationTime = {{RequireExpirationTime}},
                     SaveToken = {{SaveToken}},
                     RequireHttpsMetadata = {{RequireHttpsMetadata}}
                 }
                 """;
    }
}
