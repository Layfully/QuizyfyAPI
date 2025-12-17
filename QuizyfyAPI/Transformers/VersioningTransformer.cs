using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiOptions = QuizyfyAPI.Options.OpenApiOptions;

namespace QuizyfyAPI.Transformers;

internal sealed class VersioningTransformer : IOpenApiDocumentTransformer
{
    private readonly OpenApiOptions _options;

    public VersioningTransformer(OpenApiOptions options)
    {
        _options = options;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        string versionSegment = $"v{_options.APIVersion}"; 

        var paths = document.Paths.ToDictionary(entry => entry.Key, entry => entry.Value);
        document.Paths.Clear();

        foreach ((string pathKey, IOpenApiPathItem pathItem) in paths)
        {
            string newPathKey = pathKey
                .Replace("v{version:apiVersion}", versionSegment, StringComparison.OrdinalIgnoreCase)
                .Replace("v{version}", versionSegment, StringComparison.OrdinalIgnoreCase);
            
            foreach (OpenApiOperation operation in pathItem.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
            {
                if (operation.Parameters is null)
                {
                    continue;
                }

                var paramsToRemove = operation.Parameters
                    .Where(p => p.Name is "version" or "apiVersion")
                    .ToList();

                foreach (IOpenApiParameter p in paramsToRemove)
                {
                    operation.Parameters.Remove(p);
                }
            }

            document.Paths.Add(newPathKey, pathItem);
        }

        return Task.CompletedTask;
    }
}