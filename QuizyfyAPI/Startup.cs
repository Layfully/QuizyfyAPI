using System;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizyfyAPI.Data;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using QuizyfyAPI.Services;
using QuizyfyAPI.Middleware;
using QuizyfyAPI.Options;
using PwnedPasswords.Client;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using reCAPTCHA.AspNetCore;
using System.Net.Http;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace QuizyfyAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private SwaggerOptions SwaggerOptions;


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            SwaggerOptions = new SwaggerOptions();
            var AppOptions = new AppOptions();
            var JwtOptions = new JwtOptions();

            Configuration.GetSection(nameof(SwaggerOptions)).Bind(SwaggerOptions);
            Configuration.GetSection(nameof(AppOptions)).Bind(AppOptions);
            Configuration.GetSection(nameof(JwtOptions)).Bind(JwtOptions);

            services.AddOptions();
            services.Configure<AppOptions>(Configuration.GetSection(nameof(AppOptions)));
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));
            services.Configure<SwaggerOptions>(Configuration.GetSection(nameof(SwaggerOptions)));
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));

            services.AddCors();

            services.ConfigureDbContext(AppOptions);

            services.Scan(scan => scan.FromAssemblyOf<Startup>()
            .AddClasses(classes => classes.AssignableToAny(typeof(IService), typeof(IRepository)))
            .AsMatchingInterface().WithScopedLifetime());

            services.ConfigureApiVersioning(SwaggerOptions);

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services.ConfigureMvcForApi(AppOptions);

            services.ConfigureValidationErrorResponse();

            services.ConfigureJWTAuth(JwtOptions);

            services.ConfigureSwagger(SwaggerOptions);

            services.AddMemoryCache();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddScoped<IUrlHelper>(factory => new UrlHelper(factory.GetService<IActionContextAccessor>().ActionContext));

            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddPwnedPasswordHttpClient();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(SwaggerOptions.UIEndpoint, SwaggerOptions.Title);
                setupAction.RoutePrefix = SwaggerOptions.RoutePrefix;
            });
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
