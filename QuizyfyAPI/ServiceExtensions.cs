using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuizyfyAPI.Data;
using QuizyfyAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace QuizyfyAPI
{
    public static class ServiceExtensions
    {
        public static void ConfigureApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(0, 5);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
        }

        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<QuizDbContext>(
                options => options.UseSqlServer(
                    configuration.GetConnectionString("QuizyfyAPI")
                ));
        }

        public static void ConfigureJWTAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async (context) =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var userId = int.Parse(context.Principal.Identity.Name);
                        var user = await userService.GetUserById(userId);
                        if (user == null)
                        {
                            context.Fail("Unauthorized");
                        }
                    }
                };

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParameters;
            });
        }

        public static void ConfigureMvcForApi(this IServiceCollection services)
        {
            services.AddMvc(setupAction => {

                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));

                setupAction.ReturnHttpNotAcceptable = true;
              
                var jsonFormatter = setupAction.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                if (jsonFormatter != null)
                {
                    if (jsonFormatter.SupportedMediaTypes.Contains("text/json"))
                    {
                        jsonFormatter.SupportedMediaTypes.Remove("text/json");
                    }
                }
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public static void ConfigureValidationErrorResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options => {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var actionExecutionContext = actionContext as ActionExecutingContext;

                    if (actionContext.ModelState.ErrorCount > 0 && actionExecutionContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                    {
                        string messages = string.Join("; ", actionContext.ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                        Console.WriteLine(messages);
                        return new UnprocessableEntityObjectResult(actionContext.ModelState);
                    }

                    return new BadRequestObjectResult(actionContext.ModelState);
                };
            });
        }
    }
}
