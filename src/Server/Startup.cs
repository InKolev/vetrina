using System;
using FluentValidation.AspNetCore;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Vetrina.Server.Abstractions;
using Vetrina.Server.HostedServices;
using Vetrina.Server.Jobs;
using Vetrina.Server.Mediatr.CommandHandlers;
using Vetrina.Server.Mediatr.Pipelines;
using Vetrina.Server.Middlewares;
using Vetrina.Server.Options;
using Vetrina.Server.Persistence;
using Vetrina.Server.Services;

namespace Vetrina.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Logging.
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder
                    .ClearProviders()
                    .AddSerilog(dispose: true)
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // Filters out entity framework commands logs.
            });

            services
                .AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddCors();

            // Add Entity Framework.
            services.AddDbContext<VetrinaDbContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString(
                        "VetrinaDatabase"));
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Add Mediator services.
            var mediatorComponentsAssembly = typeof(ScrapeKauflandPromotionsCommandHandler).Assembly;
            services.AddMediatR(mediatorComponentsAssembly);
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(CommandsValidationPipeline<,>));

            // Add jobs.
            services.AddScoped<IScrapeKauflandPromotionsJob, ScrapeKauflandPromotionsJob>();
            services.AddScoped<IScrapeLidlPromotionsJob, ScrapeLidlPromotionsJob>();

            // Add Hangfire services.
            var hangfireDatabaseConnectionString = 
                Configuration.GetConnectionString("HangfireDatabase");
            
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireDatabaseConnectionString));

            services.AddHangfireServer();

            // Add background services.
            services.AddHostedService<InitializeVetrinaDatabase>();
            services.AddHostedService<InitializeLuceneIndex>();
            services.AddHostedService<InitializeHangfireJobs>();

            // Add Swagger/OpenAPI services.
            services.AddSwaggerDocument(settings =>
            {
                settings.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Vetrina API";
                    document.Info.Description = "Vetrina REST API.";
                };
            });

            // Configure options.
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));
            services.Configure<SmtpOptions>(Configuration.GetSection(nameof(SmtpOptions)));

            // Add services.
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IWebDriverFactory, ChromeWebDriverFactory>();
            services.AddSingleton<ILuceneIndex, InMemoryLuceneIndex>();
            services.AddSingleton<ITransliterationService, InMemoryTransliterationService>();

            services.AddControllersWithViews()
                .AddFluentValidation(x =>
                    x.RegisterValidatorsFromAssembly(
                        mediatorComponentsAssembly)); 

            services.AddRazorPages();

            // Creates the hangfire database if it doesn't exist.
            // TODO: It's better to move this outside of the code, and make it a step in the deployment pipeline.
            EnsureHangfireDatabaseExists(hangfireDatabaseConnectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor |
                        ForwardedHeaders.XForwardedProto
                });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable the Swagger UI middleware and the Swagger generator.
            app.UseOpenApi();

            app.UseSwaggerUi3();

            app.UseHangfireDashboard();

            app.UseHttpsRedirection();
            
            app.UseBlazorFrameworkFiles();
            
            app.UseStaticFiles();

            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            // Register global error handling middleware.
            app.UseMiddleware<ErrorHandlerMiddleware>();

            // Register custom authorization middleware.
            app.UseMiddleware<JwtMiddleware>();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHangfireDashboard();
            });
        }

        private static void EnsureHangfireDatabaseExists(string connectionString)
        {
            var options =
                new DbContextOptionsBuilder<HangfireDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

            var dbContext = new HangfireDbContext(options);

            dbContext.Database.Migrate();
        }
    }
}
