using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AHI.Infrastructure.Import.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Constant;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.Export.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.SharedKernel;

namespace AHI.AssetTable.Function.Service
{
    public class FileExportService : IFileExportService
    {
        private readonly IExportNotificationService _notification;
        private readonly IExportTrackingService _errorService;
        private readonly IParserContext _context;
        private readonly IAssetTableExportHandler _exportHandler;
        private readonly IStorageService _storageService;
        private readonly ILoggerAdapter<FileExportService> _logger;

        public FileExportService(
            IExportNotificationService notificationService,
            IExportTrackingService errorService,
            IParserContext parserContext,
           IAssetTableExportHandler exportHandler,
            IStorageService storageService,
            ILoggerAdapter<FileExportService> logger)
        {
            _notification = notificationService;
            _errorService = errorService;
            _context = parserContext;
            _exportHandler = exportHandler;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<ImportExportBasePayload> ExportFileAsync(
            Guid tableId,
            string upn,
            Guid activityId,
            ExecutionContext context,
            string filter,
            string dateTimeFormat,
            Infrastructure.UserContext.Models.TimeZone timeZone)
        {
            _context.SetDateTimeFormat(dateTimeFormat);
            _context.SetTimezoneOffset(DateTimeExtensions.ToValidOffset(timeZone.Offset));
            var objectType = Constant.IOEntityType.ASSET_TABLE;
            _notification.Upn = upn;
            _notification.ActivityId = activityId;
            _notification.ObjectName = Constant.ActivityLogEntityNames.GetActivityLogEntityName(objectType);
            _notification.NotificationType = ActionType.Export;

            await _notification.SendFinishExportNotifyAsync(ActionStatus.Start);
            try
            {
                var downloadUrl = await _exportHandler.HandleAsync(context.FunctionAppDirectory, tableId, filter);
                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    _notification.URL = downloadUrl;
                    _logger.LogInformation($"Download url: {downloadUrl}");
                }
            }
            catch (Exception e)
            {
                _errorService.RegisterError(e.Message);
                _logger.LogError(e, e.Message);
            }
            var status = GetFinishStatus();
            var payload = await _notification.SendFinishExportNotifyAsync(status);
            return CreateLogPayload(payload);
        }

        private ActionStatus GetFinishStatus()
        {
            return _errorService.HasError ? ActionStatus.Fail : ActionStatus.Success;
        }

        private ImportExportLogPayload<TrackError> CreateLogPayload(ImportExportNotifyPayload payload)
        {
            var detail = new[] { new ExportPayload<TrackError>((payload as ExportNotifyPayload).URL, _errorService.GetErrors) };
            return new ImportExportLogPayload<TrackError>(payload)
            {
                Detail = detail
            };
        }
    }
}
