using System.Globalization;
using Asp.Versioning.Builder;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Endpoints;

internal static class LikeEndpoints
{
    public static RouteGroupBuilder MapLikeEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/v{version:apiVersion}/quizzes/{quizId:int}/likes")
            .WithTags("Likes")
            .RequireAuthorization()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

        group.MapPut("/", LikeQuiz)
            .WithName("LikeQuiz")
            .WithSummary("Like a quiz")
            .WithDescription("Adds a like to the specified quiz for the currently authenticated user.");

        group.MapDelete("/", UnlikeQuiz)
            .WithName("UnlikeQuiz")
            .WithSummary("Unlike a quiz")
            .WithDescription("Removes the like from the specified quiz for the currently authenticated user.");
        
        return group;
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirst("id")?.Value ?? "0", CultureInfo.InvariantCulture);
        
    }

    // PUT /api/v1/Quizzes/{quizId}/Likes
    public static async Task<IResult> LikeQuiz(
        int quizId, 
        ClaimsPrincipal user,
        [FromServices] ILikeService likeService)
    {
        int userId = GetUserId(user);
        var result = await likeService.Like(quizId, userId);

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

    // DELETE /api/v1/Quizzes/{quizId}/Likes
    public static async Task<IResult> UnlikeQuiz(
        int quizId,
        ClaimsPrincipal user,
        [FromServices] ILikeService likeService)
    {
        int userId = GetUserId(user);
        DetailedResult result = await likeService.Delete(quizId, userId);

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