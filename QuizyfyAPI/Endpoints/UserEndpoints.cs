using System.Globalization;
using Asp.Versioning.Builder;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Filters;
using QuizyfyAPI.Services.Interfaces;

namespace QuizyfyAPI.Endpoints;

internal static class UserEndpoints
{
    private static int GetUserId(ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirst("id")?.Value ?? "0", CultureInfo.InvariantCulture);    
    }
    
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/v{version:apiVersion}/users")
            .WithTags("Users")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);
        
        // Login
        group.MapPost("/Login", LoginUser)
            .AddEndpointFilter<RecaptchaFilter>()
            .AllowAnonymous()
            .RequireRateLimiting("StrictAuth")
            .WithSummary("Authenticate user")
            .WithDescription("Authenticates a user with username/password and Recaptcha. Returns a JWT token.")
            .Accepts<UserLoginRequest>("application/json")
            .ProducesValidationProblem();
        
        // Register
        group.MapPost("/Register", RegisterUser)
            .AddEndpointFilter<RecaptchaFilter>()
            .AllowAnonymous()
            .RequireRateLimiting("StrictAuth")
            .WithSummary("Register user")
            .WithDescription("Creates a new user account. Requires Recaptcha validation.")
            .Accepts<UserRegisterRequest>("application/json")
            .ProducesValidationProblem();

        // Refresh
        group.MapPost("/Refresh", RefreshToken)
            .AllowAnonymous()
            .WithSummary("Refresh Token")
            .WithDescription("Refreshes an expired JWT using a valid Refresh Token.")
            .Accepts<UserRefreshRequest>("application/json");
        
        // Verification & Recovery (PATCH)

        group.MapPatch("/EmailVerification/{id:int}", VerifyEmail)
            .AllowAnonymous()
            .WithSummary("Verify Email")
            .WithDescription("Verifies a user's email address using a token sent via email.")
            .Accepts<VerifyEmailRequest>("application/json");

        group.MapPatch("/PasswordRecovery/{id:int}", RecoverPassword)
            .AllowAnonymous()
            .WithSummary("Recover Password")
            .WithDescription("Resets a user's password using a valid recovery token.")
            .Accepts<RecoverPasswordRequest>("application/json");

        group.MapPatch("/RecoveryTokenGeneration", GenerateRecoveryToken)
            .AllowAnonymous()
            .WithSummary("Generate Recovery Token")
            .WithDescription("Generates and emails a password recovery token to the user.")
            .Accepts<RecoveryTokenGenerationRequest>("application/json");
        
        // --- PROTECTED ENDPOINTS ---
        
        // Update
        group.MapPut("/{id:int}", UpdateUser)
            .RequireAuthorization()
            .WithSummary("Update User")
            .WithDescription("Updates user details. Users can update their own data; Admins can update anyone.")
            .Accepts<UserUpdateRequest>("application/json");

        // Delete
        group.MapDelete("/{id:int}", DeleteUser)
            .RequireAuthorization()
            .WithSummary("Delete User")
            .WithDescription(
                "Permanently deletes a user account. Users can delete themselves; Admins can delete anyone.");
        
        return group;
    }

    public static async Task<IResult> LoginUser(UserLoginRequest request, [FromServices] IUserService userService)
    {
        var result = await userService.Login(request);

        if (!result.Success)
        {
            return TypedResults.Problem(
                detail: result.Errors.FirstOrDefault(), 
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return TypedResults.Ok(result.Object);
    }
    
    public static async Task<IResult> RegisterUser(UserRegisterRequest request, [FromServices] IUserService userService)
    {
        BasicResult result = await userService.Register(request);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.Errors);
        }
        
        return TypedResults.Json(
            "User registered successfully. Please check email for verification.", 
            AppJsonSerializerContext.Default.String,
            statusCode: StatusCodes.Status201Created);
    }

    public static async Task<IResult> UpdateUser(int id, ClaimsPrincipal user, UserUpdateRequest request,
        [FromServices] IUserService userService)
    {
        if (!user.IsInRole(Role.Admin) && GetUserId(user) != id)
        {
            return TypedResults.Forbid();
        }

        var result = await userService.Update(id, request);

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
    
    public static async Task<IResult> VerifyEmail(int id, VerifyEmailRequest request, [FromServices] IUserService userService)
    {
        var result = await userService.VerifyEmail(id, request.Token);

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }

        if (result.Errors.Any(e => e.Contains("Couldn't find user", StringComparison.OrdinalIgnoreCase)))
        {
            return TypedResults.NotFound(result.Errors);
        }

        return TypedResults.BadRequest(result.Errors);
    }

    public static async Task<IResult> RecoverPassword(int id, RecoverPasswordRequest request, [FromServices] IUserService userService)
    {
        var result = await userService.RecoverPassword(id, request.Token, request.Password);

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }

        return TypedResults.BadRequest(result.Errors);
    }
    
    public static async Task<IResult> GenerateRecoveryToken(RecoveryTokenGenerationRequest request, [FromServices] IUserService userService)
    {
        var result = await userService.GenerateRecoveryToken(request);

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }

        return TypedResults.BadRequest(result.Errors);
    }
    
    public static async Task<IResult> DeleteUser(int id, ClaimsPrincipal user, [FromServices] IUserService userService)
    {
        if (!user.IsInRole(Role.Admin) && GetUserId(user) != id)
        {
            return TypedResults.Forbid();
        }

        DetailedResult result = await userService.Delete(id);

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

    public static async Task<IResult> RefreshToken(UserRefreshRequest request, [FromServices] IUserService userService)
    {
        var result = await userService.RefreshTokenAsync(request);

        if (result.Success)
        {
            return TypedResults.Ok(result.Object);
        }

        return TypedResults.BadRequest(result.Errors);
    }
}