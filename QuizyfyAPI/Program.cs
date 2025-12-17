using Microsoft.AspNetCore.Http.Timeouts;
using QuizyfyAPI;
using QuizyfyAPI.Data.Repositories;
using QuizyfyAPI.Data.Repositories.Interfaces;
using QuizyfyAPI.Extensions;
using QuizyfyAPI.Handlers;
using QuizyfyAPI.Options;
using QuizyfyAPI.ServiceDefaults;
using QuizyfyAPI.Services.Interfaces;
using reCAPTCHA.AspNetCore;
using Scalar.AspNetCore;
using SendGrid;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); 

// --- CONFIGURATION & SERVICES ---

// Load Configuration Options explicitly needed for logic during startup
OpenApiOptions openApiOptions = builder.Configuration.GetSection(nameof(OpenApiOptions)).Get<OpenApiOptions>() ?? new() { Title = "Quizyfy API" };
AppOptions appOptions = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>() ?? new() { ConnectionString = string.Empty, ServerPath = string.Empty};
JwtOptions jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>() ?? new JwtOptions { Secret = string.Empty };
RateLimitOptions rateLimitOptions = builder.Configuration.GetSection(nameof(RateLimitOptions)).Get<RateLimitOptions>() ?? new();

string sendGridApiKey = builder.Configuration["SendGridClientOptions:ApiKey"] ?? string.Empty;

// Register Options (IOptions pattern)
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection(nameof(SendGridOptions)));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(nameof(AppOptions)));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<OpenApiOptions>(builder.Configuration.GetSection(nameof(OpenApiOptions)));
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("RecaptchaSettings"));

// Standard Services
builder.AddServiceDefaults();
builder.Services.AddCors();
builder.Services.AddHybridCacheWithOptionalRedis(builder.Configuration);
builder.AddSmartOutputCache();
builder.Services.AddValidation();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCustomRateLimiting(rateLimitOptions);
builder.Services.AddRequestTimeouts(requestTimeoutOptions => { requestTimeoutOptions.DefaultPolicy = new RequestTimeoutPolicy { Timeout = TimeSpan.FromSeconds(60) }; });

// Database & Custom Extensions
builder.Services.ConfigureDbContext(appOptions);
builder.Services.ConfigureApiVersioning(openApiOptions);
builder.Services.ConfigureJWTAuth(jwtOptions);
builder.Services.ConfigureOpenApi(openApiOptions);

builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IChoiceService, ChoiceService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IUserService, UserService>();

// Third Party Services
builder.Services.AddTransient<IRecaptchaService, RecaptchaService>();
builder.Services.AddPwnedPasswordHttpClient();
builder.Services.AddHttpClient("SendGridRetryClient");

builder.Services.AddSingleton<ISendGridClient>(sp =>
{
    IHttpClientFactory httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    HttpClient resilientClient = httpClientFactory.CreateClient("SendGridRetryClient");

    return new SendGridClient(resilientClient, sendGridApiKey);
});

builder.Services.AddSingleton<ISendGridService, SendGridService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });


// --- PIPELINE (BUILD APP) ---
WebApplication app = builder.Build();
app.MapDefaultEndpoints(); 

if (app.Environment.IsDevelopment())
{
   app.UseDeveloperExceptionPage();
}
else
{
   app.UseHsts();
}

app.UseExceptionHandler(); 
app.UseHttpsRedirection();
app.UseResponseCompression(); 
app.UseStaticFiles();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseOutputCache();
app.UseRequestTimeouts();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();

if (app.Environment.IsDevelopment() || openApiOptions.EnableUiInProduction)
{
    app.MapScalarApiReference(openApiOptions.UiEndpoint, options =>
    {
        options.WithTitle(openApiOptions.Title);
        options.WithOpenApiRoutePattern(openApiOptions.JsonRoute);
        options.WithTheme(ScalarTheme.Kepler);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}


app.MapApiEndpoints(openApiOptions);

await app.EnsureDatabaseSetup();
await app.RunAsync();