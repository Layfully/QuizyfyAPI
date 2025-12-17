using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Options;
using QuizyfyAPI.Transformers;
using OpenApiOptions = QuizyfyAPI.Options.OpenApiOptions;

namespace QuizyfyAPI;

internal static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureApiVersioning(OpenApiOptions options)
        {
            services.AddApiVersioning(ver =>
            {
                ver.DefaultApiVersion = new ApiVersion(options.APIVersionMajor, options.APIVersionMinor);
                ver.ReportApiVersions = options.ReportAPIVersion;
                ver.AssumeDefaultVersionWhenUnspecified = options.SupplyDefaultVersion;
                ver.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
        }

        public void ConfigureDbContext(AppOptions appOptions)
        {
            services.AddDbContextPool<QuizDbContext>(options => 
                options.UseSqlServer(appOptions.ConnectionString, sqlOptions => 
                    {
                        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    })
                    .UseModel(Data.CompiledModels.QuizDbContextModel.Instance)
            );
        }

        public void ConfigureJWTAuth(JwtOptions jwtOptions)
        {
            byte[] key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                RequireExpirationTime = jwtOptions.RequireExpirationTime,
                ClockSkew = TimeSpan.Zero
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
                    x.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                    x.SaveToken = jwtOptions.SaveToken;
                    x.TokenValidationParameters = tokenValidationParameters;
            
                    x.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async (context) =>
                        {
                            IUserRepository userService = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                            Claim? userIdClaim = context.Principal?.FindFirst("id");
                    
                            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
                            {
                                context.Fail("Unauthorized");
                                return;
                            }
                    
                            if (await userService.GetUserById(userId) is null)
                            {
                                context.Fail("Unauthorized");
                            }
                        }
                    };
                });
        }

        public void ConfigureOpenApi(OpenApiOptions openApiOptions)
        {
            services.AddOpenApi(openApiOptions.FinalDocumentName, options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                options.AddDocumentTransformer(new ApiInfoTransformer(openApiOptions));
                options.AddDocumentTransformer<BearerSecurityTransformer>();
                options.AddDocumentTransformer<ServerUriTransformer>();
                options.AddDocumentTransformer(new VersioningTransformer(openApiOptions));
            });
        }

        public IServiceCollection AddCustomRateLimiting(RateLimitOptions rateLimitOptions)
        {
            return services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    CreateStrictPartition(rateLimitOptions, 2, TimeSpan.FromSeconds(1)),
                    CreateStrictPartition(rateLimitOptions, 10, TimeSpan.FromSeconds(30)),
                    CreateStrictPartition(rateLimitOptions, 20, TimeSpan.FromMinutes(2)),
                    CreateStrictPartition(rateLimitOptions, 100, TimeSpan.FromMinutes(15)),
                    CreateStrictPartition(rateLimitOptions, 1000, TimeSpan.FromHours(12)),
                    CreateStrictPartition(rateLimitOptions, 10000, TimeSpan.FromDays(7))
                );
            });
        }
    }

    private static PartitionedRateLimiter<HttpContext> CreateStrictPartition(RateLimitOptions settings, int limit, TimeSpan window)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            EnableRateLimitingAttribute? rateLimitMeta = context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
            if (rateLimitMeta?.PolicyName != "StrictAuth")
            {
                return RateLimitPartition.GetNoLimiter("NoLimit");
            }

            string remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (!string.IsNullOrEmpty(settings.RealIpHeader) && 
                context.Request.Headers.TryGetValue(settings.RealIpHeader, out StringValues headerIp))
            {
                remoteIp = headerIp.ToString();
            }

            if (settings.IpWhitelist.Contains(remoteIp))
            {
                return RateLimitPartition.GetNoLimiter("Whitelisted");
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: remoteIp,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = limit,
                    Window = window,
                    QueueLimit = 0
                });
        });
    }
}
