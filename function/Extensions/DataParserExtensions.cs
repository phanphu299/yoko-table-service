using Microsoft.Extensions.DependencyInjection;
using Function.Model.ImportModel;
using Function.FileParser;
using AHI.Infrastructure.Import.Extension;
using AHI.Infrastructure.Import.Abstraction;
using AHI.Infrastructure.Export.Extension;
using AHI.AssetTable.Function.Service.Abstraction;
using Function.Service.Abstraction;
using Function.Service;
using AHI.AssetTable.Function.Service;
using AHI.AssetTable.Function.FileParser.Abstraction;
using AHI.AssetTable.Function.FileParser.ErrorTracking;

namespace Function.Extension
{
    public static class DataParserExtensions
    {
        public static void AddDataParserServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddExportingServices();
            serviceCollection.AddImportingServices();
            serviceCollection.AddScoped<IFileHandler<AssetTableModel>, AssetTableExcelParser>();
            serviceCollection.AddScoped<IExportNotificationService, ExportNotificationService>();
            serviceCollection.AddScoped<IFileImportService, FileImportService>();
            serviceCollection.AddScoped<IImportNotificationService, ImportNotificationService>();
            serviceCollection.AddScoped<IImportTrackingService, ExcelTrackingService>();
        }
    }
}
