using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Service.Extension;
using AssetTable.Pipeline;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Infrastructure.OpenTelemetry;
using Prometheus;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.Service;
using AssetTable.Application.Constant;
using System;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using AHI.Infrastructure.UserContext.Extension;
using AHI.Infrastructure.Service.Dapper.Extension;
using AHI.Infrastructure.Audit.Extension;
using AHI.Infrastructure.Validation.Extension;
using FluentValidation;
using System.Linq;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Enum;

namespace AssetTable.ApplicationExtension.Extension
{
    public static class ApplicationExtension
    {
        const string SERVICE_NAME = "asset-table-service";
        public static void AddApplicationServices(this IServiceCollection serviceCollection)
        {
            // Add the fluent validator
            serviceCollection.AddApplicationValidator();
            serviceCollection.AddFrameworkServices();
            serviceCollection.AddUserContextService();
            serviceCollection.AddDapperFrameworkServices();
            serviceCollection.AddRabbitMQ(SERVICE_NAME);
            serviceCollection.AddRedisCache();
            serviceCollection.AddMediatR(typeof(ApplicationExtension).GetTypeInfo().Assembly);
            serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            serviceCollection.AddScoped<IAssetService, AssetService>();
            serviceCollection.AddScoped<IEntityLockService, EntityLockService>();
            serviceCollection.AddScoped<ITableService, TableService>();
            serviceCollection.AddScoped<IFileEventService, FileEventService>();
            serviceCollection.AddAuditLogService();
            serviceCollection.AddNotification();

            serviceCollection.AddHttpClient(HttpClientNames.CONFIGURATION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Configuration"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();

            serviceCollection.AddHttpClient(HttpClientNames.DEVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Device"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();

            serviceCollection.AddHttpClient(HttpClientNames.ENTITY_SERVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Entity"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();

            serviceCollection.AddEntityTagService(DatabaseType.Postgresql);

            serviceCollection.AddOtelTracingService(SERVICE_NAME, typeof(ApplicationExtension).Assembly.GetName().Version.ToString());

            serviceCollection.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(option =>
               {
                   option.SetResourceBuilder(
                   ResourceBuilder.CreateDefault()
                       .AddService(SERVICE_NAME, typeof(ApplicationExtension).Assembly.GetName().Version.ToString()));
                   //option.AddConsoleExporter();
                   option.AddOtlpExporter(oltp =>
                   {
                       oltp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                   });
               });
            });
        }
        public static void AddApplicationValidator(this IServiceCollection serviceCollection)
        {
            // Dynamic validation registration.
            serviceCollection.AddDynamicValidation();

            // All the validator object should be added into DI
            var assemblyType = typeof(ApplicationExtension).GetTypeInfo();
            var validators = assemblyType.Assembly.DefinedTypes.Where(x => x.IsClass && !x.IsAbstract && typeof(IValidator).IsAssignableFrom(x)).ToArray();

            foreach (var validator in validators)
            {
                // Validator is an instance of abstract validator.
                if (validator.BaseType != null && validator.BaseType.IsGenericType &&
                    validator.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                {
                    var validatorType =
                        typeof(IValidator<>).MakeGenericType(validator.BaseType.GetGenericArguments()[0]);
                    serviceCollection.AddSingleton(validatorType, validator);
                }
            }
        }
    }
}
