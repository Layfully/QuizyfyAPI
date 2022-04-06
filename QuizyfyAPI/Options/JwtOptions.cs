using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Options;
public class JwtOptions
{
    [Required]
    public string Secret { get; set; } = null!;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool ValidateIssuer { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateLifetime { get; set; } = true;
    public bool RequireExpirationTime { get; set; }
    public bool SaveToken { get; set; } = true;
    public bool RequireHttpsMetadata { get; set; }
}
