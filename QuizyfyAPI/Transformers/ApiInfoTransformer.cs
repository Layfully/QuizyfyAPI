using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiOptions = QuizyfyAPI.Options.OpenApiOptions;

namespace QuizyfyAPI.Transformers;

internal sealed class ApiInfoTransformer(OpenApiOptions options) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        OpenApiLicense license = new() { Name = options.LicenseName };

        if (!string.IsNullOrEmpty(options.LicenseURI))
        {
            license.Url = new Uri(options.LicenseURI);
        }
        
        document.Info = new OpenApiInfo
        {
            Title = options.Title,
            Version = options.APIVersion,
            Description = options.Description,
            Contact = new OpenApiContact
            {
                Email = options.ContactEmail,
                Name = options.ContactName,
            },
            License = license
        };

        return Task.CompletedTask;
    }
}