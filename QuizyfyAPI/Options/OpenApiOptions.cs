namespace QuizyfyAPI.Options;

internal sealed record OpenApiOptions
{
    // --- Versioning ---
    public int APIVersionMajor { get; init; } = 1;
    public int APIVersionMinor { get; init; }
    
    // Computed property (Not settable from config)
    public string APIVersion => $"{APIVersionMajor}.{APIVersionMinor}";
    
    public bool SupplyDefaultVersion { get; init; } = true;
    public bool ReportAPIVersion { get; init; } = true;

    // --- Routing ---
    public string DocumentName { get; init; } = "document"; 
    
    public string FinalDocumentName => $"{DocumentName}-{APIVersion}";

    // Computed: "/openapi/v1.json"
    public string JsonRoute => $"/openapi/{FinalDocumentName}.json"; 

    // Computed: "scalar/v1"
    public string UiEndpoint => $"scalar/{FinalDocumentName}";

    // --- Production Safety ---
    public bool EnableUiInProduction { get; init; }

    // --- Metadata ---
    public string Title { get; init; } = "Quizyfy API"; 
    public string Description { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? ContactEmail { get; init; }
    public string? LicenseName { get; init; }
    public string? LicenseURI { get; init; }
}
