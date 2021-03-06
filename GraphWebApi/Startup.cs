// ------------------------------------------------------------------------------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using GraphExplorerAppModeService.Services;
using GraphExplorerAppModeService.Interfaces;

namespace GraphWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _env = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration, "AzureAd")
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(Configuration.GetSection("GraphV1"))
                .AddInMemoryTokenCaches();
            services.AddDistributedMemoryCache();
            services.AddAuthentication(option =>
            {
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                   .AddJwtBearer(option =>
                   {
                       option.Authority = string.Format("{0}{1}", Configuration["AzureAd:Instance"], Configuration["AzureAd:TenantId"]);
                       option.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidAudience = Configuration["AzureAd:Audience"],
                           ValidIssuer = Configuration["AzureAd:Issuer"]
                       };
                       // This is to allow local testing using postman
                       // To be removed when merged into dev
                       option.Events = new JwtBearerEvents()
                       {
                           OnAuthenticationFailed = context =>
                           {
                               context.NoResult();
                               return Task.FromResult(0);
                           }
                       };
                   });

            //#region AppInsights

            //services.AddApplicationInsightsTelemetry(options =>
            //{
            //    options.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
            //    options.RequestCollectionOptions.InjectResponseHeaders = true;
            //    options.RequestCollectionOptions.TrackExceptions = true;
            //    options.EnableAuthenticationTrackingJavaScript = false;
            //    options.EnableHeartbeat = true;
            //    options.EnableAdaptiveSampling = true;    // Control volume of telemetry sent to AppInsights
            //    options.EnableQuickPulseMetricStream = true;   // Enable Live Metrics stream
            //    options.EnableDebugLogger = true;

            //});

            //if (!_env.IsDevelopment())
            //{
            //    services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, o) =>
            //        module.AuthenticationApiKey = Configuration["ApplicationInsights:AppInsightsApiKey"]);
            //}

            //#endregion

            services.AddMemoryCache();
            services.AddSingleton<IGraphAppAuthProvider, GraphAppAuthProvider>();
            services.AddControllers().AddNewtonsoftJson();
            // Localization
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("de"),
                    new CultureInfo("fr"),
                    new CultureInfo("es"),
                    new CultureInfo("ru"),
                    new CultureInfo("ja"),
                    new CultureInfo("pt"),
                    new CultureInfo("zh")
                };
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                DefaultContentType = "text/plain",
                ServeUnknownFileTypes = true
            });
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();

            // Localization
            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
