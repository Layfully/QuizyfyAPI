namespace QuizyfyAPI.Options
{
    public class JwtOptions
    {
        public string Secret { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool RequireExpirationTime { get; set; }
        public bool SaveToken { get; set; }
        public bool RequireHttpsMetadata { get; set; }
    }
}
