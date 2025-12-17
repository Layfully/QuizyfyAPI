using System.Net.Mime;
using Asp.Versioning.Builder;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Endpoints;

internal static class ImageEndpoints
{
    public static RouteGroupBuilder MapImageEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/v{version:apiVersion}/images")
            .WithTags("Images")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

        group.MapGet("/", GetAllImages)
            .WithName("GetAllImages")
            .WithSummary("Get list of all images")
            .WithDescription("Retrieves a collection of all uploaded images available on the server.")
            .CacheOutput("Images");

        group.MapGet("/{id:int}", GetImage)
            .WithName("GetImage")
            .WithSummary("Get one image")
            .WithDescription("Retrieves metadata for a specific image by its ID.")
            .CacheOutput("Images");

        group.MapPost("/", CreateImage)
            .WithName("CreateImage")
            .WithSummary("Upload an image")
            .WithDescription("Uploads a new image file to the server. Requires multipart/form-data.")
            .DisableAntiforgery()
            .Accepts<IFormFile>(MediaTypeNames.Multipart.FormData);

        group.MapPut("/{id:int}", UpdateImage)
            .WithName("UpdateImage")
            .WithSummary("Update an image")
            .WithDescription("Replaces an existing image file with a new one based on the provided ID.")
            .DisableAntiforgery()
            .Accepts<IFormFile>(MediaTypeNames.Multipart.FormData);

        group.MapDelete("/{id:int}", DeleteImage)
            .WithName("DeleteImage")
            .WithSummary("Delete an image")
            .WithDescription("Permanently removes an image file and its metadata from the server.");

        return group;
    }

    // GET /api/v1/Images
    public static async Task<IResult> GetAllImages([FromServices] IImageService imageService)
    {
        var result = await imageService.GetAll();

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }

        if (result.Found)
        {
            return TypedResults.NoContent();
        }
        
        return TypedResults.NotFound(result.Errors);
    }

    // GET /api/v1/Images/{id}
    public static async Task<IResult> GetImage(int id, [FromServices] IImageService imageService)
    {
        var result = await imageService.Get(id);

        if (!result.Found)
        {
            return TypedResults.NotFound(result.Errors);
        }

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }
        
        return TypedResults.Problem(
            detail: result.Errors.FirstOrDefault(), 
            statusCode: StatusCodes.Status500InternalServerError);
    }

    // POST /api/v1/Images
    public static async Task<IResult> CreateImage(IFormFile image, [FromServices] IImageService imageService)
    {
        var result = await imageService.Create(image);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        RouteValueDictionary routeValues = new()
        { 
            ["id"] = result.Object!.Id 
        };

        return TypedResults.CreatedAtRoute(
            result.Object,
            "GetImage",
            routeValues);
    }

    // PUT /api/v1/Images/{id}
    public static async Task<IResult> UpdateImage(int id, IFormFile file, [FromServices] IImageService imageService)
    {
        var result = await imageService.Update(id, file);

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }

        if (!result.Found)
        {
            return TypedResults.NotFound(result.Errors);
        }
        
        return TypedResults.BadRequest(result.Errors);
    }
    
    // DELETE /api/v1/Images/{id}
    public static async Task<IResult> DeleteImage(int id, [FromServices] IImageService imageService)
    {
        DetailedResult result = await imageService.Delete(id);

        if (result.Success)
        {
            return TypedResults.Ok();
        }

        if (!result.Found)
        {
            return TypedResults.NotFound(result.Errors);
        }
        
        return TypedResults.BadRequest(result.Errors);
    }
}