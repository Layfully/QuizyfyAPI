using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Options;
public class SwaggerOptions
{
    [Required]
    public string DocumentName { get; set; } = null!;
    public int APIVersionMajor { get; set; } = 1;
    public int APIVersionMinor { get; set; } = 0;
    public string APIVersion => $"{APIVersionMajor}.{APIVersionMinor}";
    public bool SupplyDefaultVersion { get; set; } = true;
    public bool ReportAPIVersion { get; set; } = true;
    [Required]
    public string JsonEndpoint { get; set; } = null!;
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    public string? RoutePrefix { get; set; }
    public string? LicenseName { get; set; }
    public string? LicenseURI { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
}
