using Microsoft.Extensions.Caching.Hybrid;

namespace QuizyfyAPI.Extensions;

internal static class CacheExtensions
{
    public static void AddHybridCacheWithOptionalRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(10)
            };
        });

        string? redisConnection = configuration.GetConnectionString("redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "Quizyfy_";
            });
            
            Console.WriteLine("--> Redis configured (L2 Cache enabled).");
        }
        else
        {
            Console.WriteLine("--> Redis not found. Running in L1 Memory-Only mode.");
        }
    }
    
    public static void AddSmartOutputCache(this IHostApplicationBuilder builder)
    {
        string? redisConnection = builder.Configuration.GetConnectionString("redis");
        
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            builder.AddRedisOutputCache("redis");
            Console.WriteLine("--> OutputCache: Using Redis (Aspire).");
        }
        else
        {
            Console.WriteLine("--> OutputCache: Using In-Memory (Fallback).");
        }

        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(p => p.Expire(TimeSpan.FromSeconds(15)).SetVaryByQuery("*"));

            options.AddPolicy("Quizzes", p => 
                p.Expire(TimeSpan.FromMinutes(2))
                    .SetVaryByQuery("pageNumber", "pageSize", "includeQuestions")
                    .Tag("quizzes"));

            options.AddPolicy("SingleQuiz", p => 
                p.Expire(TimeSpan.FromMinutes(2))
                    .SetVaryByQuery("includeQuestions")
                    .SetVaryByRouteValue("id")
                    .Tag("quizzes"));

            options.AddPolicy("Images", p => 
                p.Expire(TimeSpan.FromMinutes(30))
                    .Tag("images"));
        });
    }
}