using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;
using QuizyfyAPI.Options;

namespace QuizyfyAPI
{
    public static class ServiceExtensions
    {
        public static void ConfigureApiVersioning(this IServiceCollection services, SwaggerOptions swaggerOptions)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(swaggerOptions.APIVersionMajor, swaggerOptions.APIVersionMinor);
                options.ReportApiVersions = swaggerOptions.ReportAPIVersion;
                options.AssumeDefaultVersionWhenUnspecified = swaggerOptions.SupplyDefaultVersion;
            });
        }
        public static void ConfigureDbContext(this IServiceCollection services, AppOptions appOptions)
        {
            services.AddDbContextPool<QuizDbContext>(
                options => options.UseSqlServer(
                    appOptions.ConnectionString
                ));
        }
        public static void ConfigureJWTAuth(this IServiceCollection services, JwtOptions jwtOptions)
        {
            // configure jwt authentication
            var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                RequireExpirationTime = jwtOptions.RequireExpirationTime
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

                x.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                x.SaveToken = jwtOptions.SaveToken;
                x.TokenValidationParameters = tokenValidationParameters;
            });
        }
        public static void ConfigureMvcForApi(this IServiceCollection services, AppOptions appOptions)
        {
            services.AddMvc(setupAction =>
            {
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));

                setupAction.ReturnHttpNotAcceptable = appOptions.ReturnHttpNotAcceptable;

                var jsonFormatter = setupAction.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                if (jsonFormatter != null && jsonFormatter.SupportedMediaTypes.Contains("text/json"))
                {
                    jsonFormatter.SupportedMediaTypes.Remove("text/json");
                }
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        public static void ConfigureSwagger(this IServiceCollection services, SwaggerOptions swaggerOptions)
        {
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(swaggerOptions.DocumentName, new OpenApiInfo()
                {
                    Title = swaggerOptions.Title,
                    Version = swaggerOptions.APIVersion,
                    Description = swaggerOptions.Description,
                    Contact = new OpenApiContact()
                    {
                        Email = swaggerOptions.ContactEmail,
                        Name = swaggerOptions.ContactName,
                    },
                    License = new OpenApiLicense()
                    {
                        Name = swaggerOptions.LicenseName,
                        Url = new Uri(swaggerOptions.LicenseURI)
                    }
                });

                setupAction.AddSecurityDefinition("JWT bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                });

                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "JWT bearer"
                            }
                        }, new List<string>()
                    }
                });

                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                setupAction.IncludeXmlComments(xmlCommentsPath);
            });
        }
        public static void ConfigureValidationErrorResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
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
