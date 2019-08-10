namespace QuizyfyAPI.Options
{
    public class SwaggerOptions
    {
        public string DocumentName { get; set; }
        public int APIVersionMajor { get; set; }
        public int APIVersionMinor { get; set; }
        public string APIVersion => $"{APIVersionMajor}.{APIVersionMinor}";
        public bool SupplyDefaultVersion { get; set; }
        public bool ReportAPIVersion { get; set; }
        public string JsonRoute { get; set; }
        public string UIEndpoint { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RoutePrefix { get; set; }
        public string LicenseName { get; set; }
        public string LicenseURI { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
    }
}
