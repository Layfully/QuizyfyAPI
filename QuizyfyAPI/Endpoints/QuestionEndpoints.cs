using Asp.Versioning.Builder;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Endpoints;

internal static class QuestionEndpoints
{
    public static RouteGroupBuilder MapQuestionEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet)
    {
        var group = routes.MapGroup("/api/v{version:apiVersion}/quizzes/{quizId:int}/questions")
            .WithTags("Questions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

        group.MapGet("/", GetAllQuestions)
            .WithName("GetAllQuestions")
            .WithSummary("Get list of all questions")
            .WithDescription("Retrieves all questions belonging to a specific quiz. Can optionally include choices.");

        group.MapGet("/{questionId:int}", GetQuestion)
            .WithName("GetQuestion")
            .WithSummary("Get one question")
            .WithDescription("Retrieves a single question by its ID. Can optionally include choices.");

        group.MapPost("/", CreateQuestion)
            .WithName("CreateQuestion")
            .WithSummary("Create a question")
            .WithDescription("Adds a new question to the specified quiz.")
            .Accepts<QuestionCreateRequest>("application/json");

        group.MapPut("/{questionId:int}", UpdateQuestion)
            .WithName("UpdateQuestion")
            .WithSummary("Update a question")
            .WithDescription("Updates the details of an existing question.")
            .Accepts<QuestionUpdateRequest>("application/json");

        group.MapDelete("/{questionId:int}", DeleteQuestion)
            .WithName("DeleteQuestion")
            .WithSummary("Delete a question")
            .WithDescription("Permanently removes a question from the quiz.");

        return group;
    }

    // GET /api/v1/Quizzes/{quizId}/Questions
    public static async Task<IResult> GetAllQuestions(
        int quizId, 
        [FromQuery] bool includeChoices,
        [FromServices] IQuestionService questionService)
    {
        var result = await questionService.GetAll(quizId, includeChoices);

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

    // GET /api/v1/Quizzes/{quizId}/Questions/{questionId}
    public static async Task<IResult> GetQuestion(
        int quizId, 
        int questionId, 
        [FromQuery] bool includeChoices,
        [FromServices] IQuestionService questionService)
    {
        var result = await questionService.Get(quizId, questionId, includeChoices);

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
    
    // POST /api/v1/Quizzes/{quizId}/Questions
    public static async Task<IResult> CreateQuestion(
        int quizId,
        [FromBody] QuestionCreateRequest request,
        [FromServices] IQuestionService questionService)
    {
        var result = await questionService.Create(quizId, request);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        RouteValueDictionary routeValues = new()
        {
            ["quizId"] = quizId,
            ["questionId"] = result.Object!.Id
        };

        return TypedResults.CreatedAtRoute(
            result.Object,
            "GetQuestion",
            routeValues);
    }
    
    // PUT /api/v1/Quizzes/{quizId}/Questions/{questionId}
    public static async Task<IResult> UpdateQuestion(
        int quizId,
        int questionId,
        [FromBody] QuestionUpdateRequest request,
        [FromServices] IQuestionService questionService)
    {
        var result = await questionService.Update(quizId, questionId, request);

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
    
    // DELETE /api/v1/Quizzes/{quizId}/Questions/{questionId}
    public static async Task<IResult> DeleteQuestion(
        int quizId,
        int questionId,
        [FromServices] IQuestionService questionService)
    {
        DetailedResult result = await questionService.Delete(quizId, questionId);

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