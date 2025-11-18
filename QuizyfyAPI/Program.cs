using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using QuizyfyAPI;
using QuizyfyAPI.Data;
using QuizyfyAPI.Middleware;
using QuizyfyAPI.Options;
using QuizyfyAPI.Services;
using reCAPTCHA.AspNetCore;
using SendGrid;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION & SERVICES ---

// Load Configuration Options explicitly needed for logic during startup
var swaggerOptions = builder.Configuration.GetSection(nameof(SwaggerOptions)).Get<SwaggerOptions>();
var appOptions = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>();
var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
var sendGridOptions = builder.Configuration.GetSection(nameof(SendGridClientOptions)).Get<SendGridClientOptions>();

// Register Options (IOptions pattern)
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection(nameof(SendGridOptions)));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(nameof(AppOptions)));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<SwaggerOptions>(builder.Configuration.GetSection(nameof(SwaggerOptions)));
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("RecaptchaSettings"));

// Standard Services
builder.Services.AddCors();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
    .AddClasses(classes => classes.AssignableToAny(typeof(IService), typeof(IRepository)))
    .AsMatchingInterface()
    .WithScopedLifetime());

// Legacy Rate Limiting
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

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

// Middleware Pipeline
app.UseIpRateLimiting();

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

app.UseMiddleware<ExceptionMiddleware>();

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