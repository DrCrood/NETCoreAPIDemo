using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using DotNET5API.Services;
using System;
using DotNET5API.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using DotNET5API.Models;
using Microsoft.EntityFrameworkCore;
using RockLib.Logging.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DotNET5API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //Use this method to add services to the DI container.
        //package can provide a extension method to add services to IServiceCollection
        //service lifetime: Singleton -> app lifetime,  Transient -> as the class injected into, Scope -> per each http request
        public void ConfigureServices(IServiceCollection services)
        {
            //Services such as the DB context must be registered with the dependency injection container.
            //Controller Class can use DI to get the DbContext instance
            services.AddDbContext<EmployeeDBContext>(opt => opt.UseInMemoryDatabase("Employee"));
            services.AddDbContext<ProductDBContext>(opt => opt.UseSqlite("Data Source=RockDB.db"));

            //add a long running background task
            services.AddHostedService<EmployeeStatusMonitor>();

            //Add logging instance
            services.AddSingleton<IServiceLog, ServiceLog>();

            //Add authentication service with a default Scheme
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    //JWT bearer authentication performs authentication by validating a token from the request header.
                    //Configuration.Bind tries to bind a object to a configuration section
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => Configuration.Bind("JwtSettings", options))
                    //Add Cookie authentication
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => Configuration.Bind("CookieSettings", options));

            //Register a class for some customized app settings
            services.Configure<AppRunningConfigurations>(Configuration);

            //Add health checks, used by an external monitoring service or container orchestrator to check status
            services.AddHealthChecks().AddCheck<APIHealthChecks>(
                            "API health checks",
                            failureStatus: HealthStatus.Degraded,  //default is HealthStatus.Unhealthy
                            tags: new[] { "APICheck" });
            services.AddHealthChecks().AddTypeActivatedCheck<DatabaseHealthChecks>(
                            "DB health checks",
                            failureStatus: HealthStatus.Degraded,
                            tags: new[] { "DatabaseCheck"},
                            args: new object[] { 5, 10 });      //parameters can be passed to DatabaseHealthChecks

            services.AddApiVersioning( config => {
                config.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });

            services.AddControllers
                (
                    //Register the versioning control class
                    options => options.Conventions.Add(new GroupByNamespaceConvention())
                )
            .AddNewtonsoftJson(); //add default Json formatter

            services.AddLogger().AddConsoleLogProvider(options => options.SetTemplate("[{createTime(O)}] {level} Log: {message}"));

            //config swagger to differentiate API versions
            services.AddSwaggerGen(config =>
            {
                string TitleBase = "NetCore API demo";
                string description = "NetCore API demo including swagger and xUnit";
                var TermsOfService = new Uri("https://rock.com/user/termsofservices/");
                var License = new OpenApiLicense() { Name = "MIT License" };
                var Contact = new OpenApiContact() { Name = "Rolling Rock", Email = "support@Rock.com", Url = new Uri("https://rock.com") };
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = TitleBase + " v1",
                    Description = description,
                    TermsOfService = TermsOfService,
                    License = License,
                    Contact = Contact
                });

                config.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = TitleBase + " v2",
                    Description = description,
                    TermsOfService = TermsOfService,
                    License = License,
                    Contact = Contact
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //The order of middlewares they are added in the pipeline is the order they are executed. 
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    //swagger endpoint for different versions
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetCodeAPI v1");
                   c.SwaggerEndpoint("/swagger/v2/swagger.json", "NetCodeAPI v2");
                });
            }

            //a user defined middleware
            app.UseTokenChecker();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();

            //Register the middleware for authentication, add this after app.UseRouting() and before any middlewares that need authentication to access.
            //Enable authentication and use the service registered in the DI container.
            app.UseAuthentication();

            app.UseAuthorization();

            //a user defined middleware
            app.UseRequestResponseTime();

            //add a middleware by providing a delegate directly
            //Can use app.Run(), app.Use() and app.Map to register middleware delegates
            //app.Run(async context => { await context.Response.WriteAsync("app.run( delegate ) middleware end the pipeline."); });

            app.Use( async (context, next) =>
            {
                Console.WriteLine("app.use( delegate ) middleware add a customized logic to the pipeline.");
                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                //endpoint used for health checks
                endpoints.MapHealthChecks("/api/v1/apihealth", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("APICheck")
                });
                endpoints.MapHealthChecks("/api/v2/dbhealth", new HealthCheckOptions() 
                {
                    Predicate = (check) => check.Tags.Contains("DatabaseCheck")
                });
                //with some extra requirements
                //endpoints.MapHealthChecks("/health").RequireAuthorization().RequireHost("www.test.com:5001");
            });
        }
    }
}
