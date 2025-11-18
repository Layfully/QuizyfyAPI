namespace QuizyfyAPI.Options;

public record SwaggerOptions
{
    // --- Versioning ---
    public int APIVersionMajor { get; init; } = 1;
    public int APIVersionMinor { get; init; } = 0;
    
    public string APIVersion => $"{APIVersionMajor}.{APIVersionMinor}";
    
    public bool SupplyDefaultVersion { get; init; } = true;
    public bool ReportAPIVersion { get; init; } = true;

    // --- Routing
    // Default: "v1"
    public string DocumentName { get; init; } = "v1"; 
    
    // Default: "swagger/{documentName}/swagger.json"
    public string JsonRoute { get; init; } = "swagger/{documentName}/swagger.json"; 
    
    // Default: "v1/swagger.json"
    public string UIEndpoint { get; init; } = "v1/swagger.json"; 
    
    // Default: "" (Root) or "swagger"
    public string RoutePrefix { get; init; } = string.Empty;

    // --- Metadata ---
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;

    // --- Optional Fields (Nullable) ---
    public string? LicenseName { get; init; }
    public string? LicenseURI { get; init; }
    public string? ContactName { get; init; }
    public string? ContactEmail { get; init; }
}
