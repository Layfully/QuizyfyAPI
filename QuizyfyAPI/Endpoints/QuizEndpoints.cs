using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using QuizyfyAPI.Contracts.Responses.Pagination;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Endpoints;

internal static class QuizEndpoints
{
    // The Route Group for all Quiz operations
    public static RouteGroupBuilder MapQuizEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/v{version:apiVersion}/quizzes")
            .WithTags("Quizzes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

        group.MapGet("/", GetAllQuizzes)
            .WithName("GetAllQuizzes")
            .WithSummary("Get list of all quizzes")
            .WithDescription("Retrieves a paginated list of quizzes. Supports HATEOAS links.")
            .CacheOutput("Quizzes");

        group.MapGet("/{id:int}", GetQuiz)
            .WithName("GetQuiz")
            .WithSummary("Get one quiz")
            .WithDescription("Retrieves a single quiz by its ID. Optionally includes questions and choices.")
            .CacheOutput("SingleQuiz");

        group.MapPost("/", CreateQuiz)
            .WithName("CreateQuiz")
            .WithSummary("Create a quiz")
            .WithDescription("Creates a new quiz.")
            .Accepts<QuizCreateRequest>("application/json");

        group.MapPut("/{id:int}", UpdateQuiz)
            .WithName("UpdateQuiz")
            .WithSummary("Update a quiz")
            .WithDescription("Updates an existing quiz by its ID.")
            .Accepts<QuizUpdateRequest>("application/json")
            .Produces<QuizResponse>();

        group.MapDelete("/{id:int}", DeleteQuiz)
            .WithName("DeleteQuiz")
            .WithSummary("Delete a quiz")
            .WithDescription("Permanently removes a quiz by its ID.");

        return group;
    }

    // GET /api/v1/Quizzes
    public static async Task<Results<Ok<QuizListResponse>, NoContent, NotFound<IEnumerable<string>>>> GetAllQuizzes(
        HttpContext httpContext,
        [FromServices] IQuizService quizService,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        PagingParams pagingParams = new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var result = await quizService.GetAll(pagingParams, httpContext);

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);        }

        if (result.Found)
        {
            return TypedResults.NoContent();
        }
        
        return TypedResults.NotFound(result.Errors);
    }

    // GET /api/v1/Quizzes/{id}
    public static async Task<Results<Ok<QuizResponse>, NotFound<IEnumerable<string>>>> GetQuiz(
        int id,
        [FromQuery] bool includeQuestions,
        [FromServices] IQuizService quizService)
    {
        var result = await quizService.Get(id, includeQuestions);

        if (!result.Found)
        {
            return TypedResults.NotFound(result.Errors);
        }

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }
        
        return TypedResults.NotFound(result.Errors);
    }
    
    // POST /api/v1/Quizzes
    public static async Task<Results<CreatedAtRoute<QuizResponse>, BadRequest<IEnumerable<string>>>> CreateQuiz(
        [FromBody] QuizCreateRequest request,
        [FromServices] IQuizService quizService)
    {
        var result = await quizService.Create(request);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        RouteValueDictionary routeValues = new() { ["id"] = result.Object.Id };
        return TypedResults.CreatedAtRoute(result.Object, "GetQuiz", routeValues);
    }
    
    // PUT /api/v1/Quizzes/{id}
    public static async Task<Results<Ok<QuizResponse>, NotFound<IEnumerable<string>>, BadRequest<IEnumerable<string>>>> UpdateQuiz(
        int id,
        [FromBody] QuizUpdateRequest request,
        [FromServices] IQuizService quizService)
    {
        var result = await quizService.Update(id, request);

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
    
    // DELETE /api/v1/Quizzes/{id}
    public static async Task<IResult> DeleteQuiz(int id, [FromServices] IQuizService quizService)
    {
        DetailedResult result = await quizService.Delete(id);

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