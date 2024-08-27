using System;
using AHI.Infrastructure.MultiTenancy.Extension;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.OpenTelemetry;
using AHI.Infrastructure.Cache.Redis.Extension;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using AHI.AssetTable.Function.Constant;
using AHI.AssetTable.Function.Service;
using AHI.Infrastructure.SharedKernel;
using Function.Service.Abstraction;
using Function.Service;
using Function.Model.ImportModel;
using Function.Extension;
using AHI.Infrastructure.Import.Abstraction;
using Function.Repository;
using AHI.Infrastructure.Audit.Extension;
using Function.FileParser;
using AHI.Infrastructure.Service.Dapper.Extension;
using AHI.Infrastructure.Service.Tag.Extension;

[assembly: FunctionsStartup(typeof(AssetTable.Function.Startup))]
namespace AssetTable.Function
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
        }

        public const string SERVICE_NAME = "asset-table-function";
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDataParserServices();
            builder.Services.AddHttpClient(ClientNameConstant.MASTER_FUNCTION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Function:Master"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(ClientNameConstant.NOTIFICATION_HUB, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["NotificationHubEndpoint"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(ClientNameConstant.DEVICE_SERVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Device"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(ClientNameConstant.USER_FUNCTION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Function:User"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            // builder.Services.AddHttpClient(ClientNameConstant.STORAGE_FUNCTION, (service, client) =>
            // {
            //     var configuration = service.GetRequiredService<IConfiguration>();
            //     client.BaseAddress = new Uri(configuration["Function:Storage"]);
            // }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(ClientNameConstant.STORAGE_SERVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Storage"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            // download link should be fully qualified URL, don't need to setup BaseAddress
            // download client must not use ClientCrendetialAuthentication handler to avoid Authorization conflict when download from blob storage
            builder.Services.AddHttpClient(ClientNameConstant.DOWNLOAD_CLIENT);

            builder.Services.AddScoped<ITableService, TableService>();
            builder.Services.AddScoped<IStorageService, StorageService>();

            builder.Services.AddDapperFrameworkServices();
            builder.Services.AddEntityTagService(AHI.Infrastructure.Service.Tag.Enum.DatabaseType.Postgresql);

            // add import repository services
            builder.Services.AddScoped<IAssetTableService, AssetTableService>();
            builder.Services.AddScoped<IFileHandler<AssetTableModel>, AssetTableExcelParser>();
            builder.Services.AddScoped<IEntityImportRepository<AssetTableModel>, AssetTableRepository>();
            // builder.Services.AddScoped<IDictionary<Type, IFileImport>>(service =>
            // {
            //     // return the proper type
            //     var assetTable = service.GetRequiredService<IAssetTableService>();
            //     return new Dictionary<Type, IFileImport>()
            //     {
            //         {typeof(AssetTableModel), assetTable}
            //     };
            // });

            builder.Services.AddScoped<IFileExportService, FileExportService>();
            builder.Services.AddScoped<IAssetTableExportHandler, AssetTableExportHandler>();
            // builder.Services.AddScoped<IDictionary<string, IAssetTableExportHandler>>(service =>
            // {
            //     var dictionary = new Dictionary<string, IAssetTableExportHandler>();
            //     var assetTableExportHandler = service.GetRequiredService<AssetTableExportHandler>();
            //     dictionary[IOEntityType.ASSET_TABLE] = assetTableExportHandler;
            //     return dictionary;
            // });
            builder.Services.AddNotification();
            builder.Services.AddMultiTenantService();
            builder.Services.AddRabbitMQ(SERVICE_NAME);
            builder.Services.AddRedisCache();
            builder.Services.AddAuditLogService();
            builder.Services.AddLoggingService();
            builder.Services.AddOtelTracingService(SERVICE_NAME, typeof(Startup).Assembly.GetName().Version.ToString());
            builder.Services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(option =>
               {
                   option.SetResourceBuilder(
                   ResourceBuilder.CreateDefault()
                       .AddService(SERVICE_NAME, typeof(Startup).Assembly.GetName().Version.ToString()));
                   //option.AddConsoleExporter();
                   option.AddOtlpExporter(oltp =>
                   {
                       oltp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                   });
               });
            });
        }
    }
}
