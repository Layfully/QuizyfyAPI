using QuizyfyAPI.Contracts;
using reCAPTCHA.AspNetCore;

namespace QuizyfyAPI.Filters;

internal sealed class RecaptchaFilter(IRecaptchaService recaptchaService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        IRecaptchaRequest? requestArg = context.Arguments.OfType<IRecaptchaRequest>().FirstOrDefault();

        if (requestArg is null)
        {
            return Results.Problem(
                title: "Server Configuration Error",
                detail: "Recaptcha validation was requested but no token was found in the request parameters.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        RecaptchaResponse? recaptcha = await recaptchaService.Validate(requestArg.RecaptchaToken);

        if (!recaptcha.success || recaptcha.score < 0.5)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Recaptcha", ["Recaptcha validation failed or score too low."] }
            });
        }
        
        return await next(context);
    }
}