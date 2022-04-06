using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Data;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using QuizyfyAPI.Services;
using QuizyfyAPI.Middleware;
using QuizyfyAPI.Options;
using AspNetCoreRateLimit;
using reCAPTCHA.AspNetCore;
using SendGrid;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace QuizyfyAPI;

public class Startup
{
    public IConfiguration Configuration { get; }
    private readonly SwaggerOptions SwaggerOptions;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        SwaggerOptions = new();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        AppOptions AppOptions = new();
        JwtOptions JwtOptions = new();
        SendGridClientOptions SendGridOptions = new();

        Configuration.GetSection(nameof(SwaggerOptions)).Bind(SwaggerOptions);
        Configuration.GetSection(nameof(AppOptions)).Bind(AppOptions);
        Configuration.GetSection(nameof(JwtOptions)).Bind(JwtOptions);
        Configuration.GetSection(nameof(SendGridClientOptions)).Bind(SendGridOptions);

        services.AddOptions<SendGridOptions>().Bind(Configuration.GetSection(nameof(SendGridOptions))).ValidateDataAnnotations();
        services.AddOptions<AppOptions>().Bind(Configuration.GetSection(nameof(AppOptions))).ValidateDataAnnotations();
        services.AddOptions<JwtOptions>().Bind(Configuration.GetSection(nameof(JwtOptions))).ValidateDataAnnotations();
        services.AddOptions<SwaggerOptions>().Bind(Configuration.GetSection(nameof(SwaggerOptions))).ValidateDataAnnotations();
        services.AddOptions<IpRateLimitOptions>().Bind(Configuration.GetSection("IpRateLimiting")).ValidateDataAnnotations();
        services.AddOptions<IpRateLimitPolicies>().Bind(Configuration.GetSection("IpRateLimitPolicies")).ValidateDataAnnotations();
        services.AddOptions<IpRateLimitPolicies>().Bind(Configuration.GetSection(nameof(RecaptchaSettings))).ValidateDataAnnotations();

        services.AddCors();

        services.ConfigureDbContext(AppOptions);

        services.Scan(scan => scan.FromAssemblyOf<Startup>()
        .AddClasses(classes => classes.AssignableToAny(typeof(IService), typeof(IRepository)))
        .AsMatchingInterface().WithScopedLifetime());

        services.ConfigureApiVersioning(SwaggerOptions);

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        services.ConfigureControllersForApi(AppOptions);

        services.ConfigureValidationErrorResponse();

        services.ConfigureJWTAuth(JwtOptions);

        services.ConfigureSwagger(SwaggerOptions);

        services.AddMemoryCache();
        
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        services.AddScoped<IUrlHelper>(factory => new UrlHelper(factory.GetService<IActionContextAccessor>()!.ActionContext!));

        services.AddTransient<IRecaptchaService, RecaptchaService>();
        services.AddPwnedPasswordHttpClient();
        services.AddSingleton<ISendGridClient, SendGridClient>(_ => new SendGridClient(SendGridOptions));
        services.AddSingleton<ISendGridService, SendGridService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseIpRateLimiting();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors(options =>
        {
            options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();

        app.UseSwaggerUI(setupAction =>
        {
            setupAction.SwaggerEndpoint(SwaggerOptions.JsonEndpoint, SwaggerOptions.Title);
            setupAction.RoutePrefix = SwaggerOptions.RoutePrefix ?? string.Empty;
        });

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
