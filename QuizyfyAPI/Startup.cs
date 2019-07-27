using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuizyfyAPI.Data;
using QuizyfyAPI.Models;
using System.Text;
using QuizyfyAPI.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace QuizyfyAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.ConfigureDbContext(Configuration);

            services.AddTransient<IQuizRepository, QuizRepository>();
            services.AddTransient<IQuestionRepository, QuestionRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IChoiceRepository, ChoiceRepository>();

            services.ConfigureApiVersioning();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.ConfigureMvcForApi();

            services.ConfigureValidationErrorResponse();

            services.ConfigureJWTAuth(Configuration);

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("QuizyfyOpenAPISpecification", new OpenApiInfo()
                {
                    Title = "Quizyfy API",
                    Version = "0.2",
                    Description = "Through this API you can create, access and modify existing quizzes.",
                    Contact = new OpenApiContact()
                    {
                        Email = "adriangaborek3@gmail.com",
                        Name = "Adrian Gaborek",
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
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

            services.AddResponseCompression(options =>
           {
               options.EnableForHttps = true;
           });

            services.AddMemoryCache();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                           .ActionContext;
                return new UrlHelper(actionContext);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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

            app.ConfigureGlobalExtensionHandling();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            app.UseResponseCompression();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/QuizyfyOpenAPISpecification/swagger.json", "Quzify API");
                setupAction.RoutePrefix = "";
            });
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
