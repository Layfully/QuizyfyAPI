using Asp.Versioning;
using Asp.Versioning.Builder;
using QuizyfyAPI.Options;

namespace QuizyfyAPI.Extensions;

internal static class EndpointExtensions
{
    public static void MapApiEndpoints(this WebApplication app, OpenApiOptions options)
    {
        ApiVersion apiVersion = new(options.APIVersionMajor, options.APIVersionMinor);
        
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion)
            .ReportApiVersions()
            .Build();
        
        app.MapChoiceEndpoints(versionSet);
        app.MapImageEndpoints(versionSet);
        app.MapLikeEndpoints(versionSet);
        app.MapQuestionEndpoints(versionSet);
        app.MapQuizEndpoints(versionSet);
        app.MapUserEndpoints(versionSet);
    }
}