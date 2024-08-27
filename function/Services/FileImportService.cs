using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AHI.Infrastructure.Import.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Function.Service.Abstraction;
using System.Linq;
using AHI.AssetTable.Function.Constant;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Constant;
using System.IO;
using AHI.Infrastructure.SharedKernel;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.Audit.Model;
using AHI.AssetTable.Function.FileParser.Abstraction;
using AHI.Infrastructure.Audit.Constant;
using AHI.AssetTable.Function.FileParser.ErrorTracking.Model;

namespace Function.Service
{
    public class FileImportService : IFileImportService
    {
        private readonly IImportNotificationService _notification;
        private readonly IParserContext _context;
        private readonly IAssetTableService _assetTableService;
        private readonly IStorageService _storageService;
        private readonly ILoggerAdapter<FileImportService> _logger;
        private readonly IImportTrackingService _errorService;
        public FileImportService(
            IImportNotificationService notification,
            IImportTrackingService errorService,
            IAssetTableService assetTableService,
            IParserContext context,
            IStorageService storageService,
            ILoggerAdapter<FileImportService> logger)
        {
            _notification = notification;
            _errorService = errorService;
            _context = context;
            _assetTableService = assetTableService;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<ImportExportBasePayload> ImportFileAsync(string upn, Guid activityId, string workingDirectory, Guid tableId, IEnumerable<string> fileNames, string dateTimeFormat, AHI.Infrastructure.UserContext.Models.TimeZone timeZone)
        {
            var objectType = IOEntityType.ASSET_TABLE;

            _context.SetWorkingDirectory(workingDirectory);
            _context.SetDateTimeFormat(dateTimeFormat);
            _context.SetTimezoneOffset(DateTimeExtensions.ToValidOffset(timeZone.Offset));
            _notification.Upn = upn;
            _notification.ActivityId = activityId;
            _notification.ObjectType = objectType;
            _notification.NotificationType = ActionType.Import;

            // remove token and duplicate files
            var files = PreProcessFileNames(fileNames);

            // send signalR starting import
            await _notification.SendStartNotifyAsync(fileNames.Count());
            foreach (string file in files)
            {
                _errorService.File = file;
                using (var stream = new MemoryStream())
                {
                    await DownloadImportFileAsync(file, stream);
                    if (stream.CanRead)
                    {
                        try
                        {
                            // var fileImport = _importHandlers[modelType];
                            await _assetTableService.RunImportAsync(tableId, stream);

                        }
                        catch (Exception ex)
                        {
                            _errorService.RegisterError(ex.Message, ErrorType.UNDEFINED);
                            _logger.LogError(ex, ex.Message);
                        }
                    }
                }
            }
            var status = GetFinishStatus(out var partialInfo);
            var payload = await _notification.SendFinishImportNotifyAsync(status, partialInfo);
            return CreateLogPayload(payload);
        }

        private async Task DownloadImportFileAsync(string filePath, Stream outputStream)
        {
            try
            {
                await _storageService.DownloadFileToStreamAsync(filePath, outputStream);
            }
            catch
            {
                outputStream.Dispose();
                _errorService.RegisterError(ImportErrorMessage.IMPORT_ERROR_GET_FILE_FAILED, ErrorType.UNDEFINED);
            }
        }

        private IEnumerable<string> PreProcessFileNames(IEnumerable<string> fileNames)
        {
            return fileNames.Select(x => x.RemoveFileToken()).Distinct();
        }


        private ActionStatus GetFinishStatus(out (int Success, int Fail) partialInfo)
        {
            var total = _errorService.FileErrors.Keys.Count;
            var failCount = _errorService.FileErrors.Count(x => x.Value.Count > 0);
            if (failCount == 0)
            {
                partialInfo = (total, 0);
                return ActionStatus.Success;
            }

            if (failCount == total)
            {
                partialInfo = (0, total);
                return ActionStatus.Fail;
            }

            var successCount = total - failCount;
            partialInfo = (successCount, failCount);
            return ActionStatus.Partial;
        }

        private ImportExportLogPayload<TrackError> CreateLogPayload(ImportExportNotifyPayload payload)
        {
            return new ImportExportLogPayload<TrackError>(payload)
            {
                Detail = _errorService.FileErrors.Select(x => new ImportPayload<TrackError>(x.Key, x.Value)).ToArray()
            };
        }
    }
}
