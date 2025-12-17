using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace QuizyfyAPI.Transformers;

internal sealed class ServerUriTransformer(IWebHostEnvironment env, IConfiguration config) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        if (!env.IsProduction())
        {
            return Task.CompletedTask;
        }
        
        //TODO: Change to prod url
        string productionUrl = config["OpenApiOptions:ServerUrl"] ?? "https://api.quizyfy.com";

        document.Servers = 
        [
            new OpenApiServer 
            { 
                Url = productionUrl, 
                Description = "Production Server" 
            }
        ];

        return Task.CompletedTask;
    }
}