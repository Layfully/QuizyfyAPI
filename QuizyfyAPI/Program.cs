using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.RateLimiting;
using QuizyfyAPI;
using QuizyfyAPI.Data;
using QuizyfyAPI.Handlers;
using QuizyfyAPI.Options;
using QuizyfyAPI.Services;
using reCAPTCHA.AspNetCore;
using SendGrid;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- CONFIGURATION & SERVICES ---

// Load Configuration Options explicitly needed for logic during startup
SwaggerOptions swaggerOptions = builder.Configuration.GetSection(nameof(SwaggerOptions)).Get<SwaggerOptions>() ?? new() { Title = "Quizyfy API" };
AppOptions appOptions = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>() ?? new() { ConnectionString = string.Empty, ServerPath = string.Empty};
JwtOptions jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>() ?? new JwtOptions { Secret = string.Empty };
SendGridClientOptions sendGridOptions = builder.Configuration.GetSection(nameof(SendGridClientOptions)).Get<SendGridClientOptions>() ?? new();
RateLimitOptions rateLimitOptions = builder.Configuration.GetSection(nameof(RateLimitOptions)).Get<RateLimitOptions>() ?? new();

// Register Options (IOptions pattern)
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection(nameof(SendGridOptions)));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(nameof(AppOptions)));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<SwaggerOptions>(builder.Configuration.GetSection(nameof(SwaggerOptions)));
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("RecaptchaSettings"));

// Standard Services
builder.Services.AddCors();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddRateLimiter(options =>
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

// Database & Custom Extensions
builder.Services.ConfigureDbContext(appOptions);
builder.Services.ConfigureApiVersioning(swaggerOptions);
builder.Services.ConfigureControllersForApi(appOptions);
builder.Services.ConfigureValidationErrorResponse();
builder.Services.ConfigureJWTAuth(jwtOptions);
builder.Services.ConfigureSwagger(swaggerOptions);

// Scrutor (Still excellent in .NET 10)
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes
            .AssignableToAny(typeof(IService), typeof(IRepository))
            .Where(c => !c.IsAbstract)
        , publicOnly: false)
    .AsMatchingInterface()
    .WithScopedLifetime());


// MVC Legacy Helpers (IUrlHelper)
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IUrlHelper>(x => {
    var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
    var factory = x.GetRequiredService<IUrlHelperFactory>();
    return factory.GetUrlHelper(actionContext);
});

// Third Party Services
builder.Services.AddTransient<IRecaptchaService, RecaptchaService>();
builder.Services.AddPwnedPasswordHttpClient();
builder.Services.AddSingleton<ISendGridClient>(_ => new SendGridClient(sendGridOptions));
builder.Services.AddSingleton<ISendGridService, SendGridService>();

// --- PIPELINE (BUILD APP) ---
   
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.UseDeveloperExceptionPage();
}
else
{
   app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// No explicit UseRouting() needed in .NET 6+ usually, it's automatic
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseRateLimiter();

app.UseExceptionHandler(); 

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(setupAction =>
{
   setupAction.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Title);
   setupAction.RoutePrefix = swaggerOptions.RoutePrefix;
});

// Map Controllers (Legacy)
app.MapControllers();

// Map Minimal APIs (Future)
// app.MapGet("/api/example", () => Results.Ok("Hello World"));

app.Run();


static PartitionedRateLimiter<HttpContext> CreateStrictPartition(RateLimitOptions settings, int limit, TimeSpan window)
{
    return PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // 1. Check Scope: Does this endpoint request the "StrictAuth" policy?
        // If the Controller doesn't have [EnableRateLimiting("StrictAuth")], skip this limit completely.
        var rateLimitMeta = context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
        if (rateLimitMeta?.PolicyName != "StrictAuth")
        {
            return RateLimitPartition.GetNoLimiter("NoLimit");
        }

        // 2. Identify IP: Check Header first (X-Real-IP), then Connection
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!string.IsNullOrEmpty(settings.RealIpHeader) && 
            context.Request.Headers.TryGetValue(settings.RealIpHeader, out var headerIp))
        {
            remoteIp = headerIp.ToString();
        }

        // 3. Whitelist: Check if IP is allowed
        if (settings.IpWhitelist?.Contains(remoteIp) == true)
        {
            return RateLimitPartition.GetNoLimiter("Whitelisted");
        }

        // 4. Enforce Limit: Apply the specific rule (e.g., 2 per 1s)
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