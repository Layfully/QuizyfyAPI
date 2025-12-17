using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace QuizyfyAPI.Transformers;

internal sealed class BearerSecurityTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.All(authScheme => authScheme.Name != "Bearer"))
        {
            return;
        }

        OpenApiSecurityScheme bearerScheme = new()
        {
            Name = "Authorization",
            Description = "Enter 'Bearer' [space] and then your token.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        document.Components ??= new OpenApiComponents();
        document.AddComponent("Bearer", bearerScheme);

        OpenApiSecurityRequirement securityRequirement = new()
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        foreach (OpenApiOperation operation in document.Paths.Values.SelectMany(path => path.Operations!.Values))
        {
            operation.Security ??= [];
            operation.Security.Add(securityRequirement);
        }
    }
}