using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using AHI.Infrastructure.Exception.Filter;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.MultiTenancy.Middleware;
using AHI.Infrastructure.SharedKernel;
using AHI.Infrastructure.UserContext;
using AHI.Infrastructure.UserContext.Extension;
using AHI.Infrastructure.Validation.Extension;

using AssetTable.ApplicationExtension.Extension;
using AssetTable.Persistence.Extension;

using Prometheus;
using Prometheus.SystemMetrics;

namespace AssetTable.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment;
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPersistenceService();
            services.AddMultiTenantService();
            services.AddUserContextService();
            services.AddDynamicValidation();
            services.AddLoggingService();
            services.AddApplicationServices();
            services.AddControllers(option =>
            {
                option.ExceptionHandling();

            }).AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.NullValueHandling = AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting.NullValueHandling;
                option.SerializerSettings.DateFormatString = AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting.DateFormatString;
                option.SerializerSettings.ReferenceLoopHandling = AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting.ReferenceLoopHandling;
                option.SerializerSettings.DateParseHandling = AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting.DateParseHandling;
                option.SerializerSettings.ContractResolver = AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting.ContractResolver;
            });
            services.AddAuthentication()
                .AddIdentityServerAuthentication("oidc",
                jwtTokenOption =>
                {
                    jwtTokenOption.Authority = Configuration["Authentication:Authority"];
                    jwtTokenOption.RequireHttpsMetadata = Configuration["Authentication:Authority"].StartsWith("https");
                    jwtTokenOption.TokenValidationParameters.ValidateAudience = false;
                    jwtTokenOption.ClaimsIssuer = Configuration["Authentication:Issuer"];
                }, referenceTokenOption =>
                {
                    referenceTokenOption.IntrospectionEndpoint = Configuration["Authentication:IntrospectionEndpoint"];
                    referenceTokenOption.ClientId = Configuration["Authentication:ApiScopeName"];
                    referenceTokenOption.ClientSecret = Configuration["Authentication:ApiScopeSecret"];
                    referenceTokenOption.ClaimsIssuer = Configuration["Authentication:Issuer"];
                    referenceTokenOption.SaveToken = true;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", Configuration["Authentication:ApiScopeName"]);
                });
            });
            //services.AddApplicationInsightsTelemetry();
            services.AddSystemMetrics();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWhen(
              context => !(context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/metrics")),
              builder =>
              {
                  builder.UseMiddleware<MultiTenancyMiddleware>();
                  builder.UseMiddleware<UserContextMiddleware>();
              });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapControllers()
                        .RequireAuthorization("ApiScope");
            });
        }
    }
}
