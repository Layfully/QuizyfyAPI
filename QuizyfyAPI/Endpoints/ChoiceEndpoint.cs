using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Endpoints;

internal static class ChoiceEndpoints
{
    public static RouteGroupBuilder MapChoiceEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/v{version:apiVersion}/quizzes/{quizId:int}/questions/{questionId:int}/choices")
            .WithTags("Choices")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

        group.MapGet("/", GetAllChoices)
            .WithName("GetAllChoices")
            .WithSummary("Get list of all choices")
            .WithDescription("Retrieves all choices belonging to a specific question in a quiz.");

        group.MapGet("/{choiceId:int}", GetChoice)
            .WithName("GetChoice")
            .WithSummary("Get one choice")
            .WithDescription("Retrieves a single choice by its ID.");
        
        group.MapPost("/", CreateChoice)
            .WithName("CreateChoice")
            .WithSummary("Create a choice")
            .WithDescription("Adds a new choice to the specified question.")
            .Accepts<ChoiceCreateRequest>("application/json");

        group.MapPut("/{choiceId:int}", UpdateChoice)
            .WithName("UpdateChoice")
            .WithSummary("Update a choice")
            .WithDescription("Updates the details of an existing choice.")
            .Accepts<ChoiceUpdateRequest>("application/json");

        group.MapDelete("/{choiceId:int}", DeleteChoice)
            .WithName("DeleteChoice")
            .WithSummary("Delete a choice")
            .WithDescription("Permanently removes a choice from the question.")
            .Produces(StatusCodes.Status200OK);

        return group;
    }

    // GET /api/v1/Quizzes/{qId}/Questions/{qsId}/Choices
    public static async Task<Results<Ok<ChoiceResponse[]>, NoContent, NotFound<IEnumerable<string>>>> GetAllChoices(
        int quizId, 
        int questionId, 
        [FromServices] IChoiceService choiceService)
    {
        var result = await choiceService.GetAll(quizId, questionId);

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

    // GET /api/v1/Quizzes/{qId}/Questions/{qsId}/Choices/{cId}
    public static async Task<Results<Ok<ChoiceResponse>, NotFound<IEnumerable<string>>, ProblemHttpResult>> GetChoice(
        int quizId, 
        int questionId, 
        int choiceId, 
        [FromServices] IChoiceService choiceService)
    {
        var result = await choiceService.Get(quizId, questionId, choiceId);

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
    
    // POST /api/v1/Quizzes/{qId}/Questions/{qsId}/Choices
    public static async Task<Results<CreatedAtRoute<ChoiceResponse>, BadRequest<IEnumerable<string>>>> CreateChoice(
        int quizId,
        int questionId,
        ChoiceCreateRequest request,
        [FromServices] IChoiceService choiceService)
    {
        var result = await choiceService.Create(quizId, questionId, request);

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
            "GetChoice",
            routeValues);
    }
    
    // PUT /api/v1/Quizzes/{qId}/Questions/{qsId}/Choices/{cId}
    public static async Task<Results<Ok<ChoiceResponse>, NotFound<IEnumerable<string>>, BadRequest<IEnumerable<string>>>> UpdateChoice(
        int quizId,
        int questionId,
        int choiceId,
        [FromServices] IChoiceService choiceService,
        ChoiceUpdateRequest request)
    {
        var result = await choiceService.Update(quizId, questionId, choiceId, request);

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
    
    // DELETE /api/v1/Quizzes/{qId}/Questions/{qsId}/Choices/{cId}
    public static async Task<Results<Ok, NotFound<IEnumerable<string>>, BadRequest<IEnumerable<string>>>> DeleteChoice(
        int quizId,
        int questionId,
        int choiceId,
        [FromServices] IChoiceService choiceService)
    {
        DetailedResult result = await choiceService.Delete(quizId, questionId, choiceId);

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