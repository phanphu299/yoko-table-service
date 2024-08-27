using System.Threading.Tasks;
using AHI.AssetTable.Function.Model;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.UserContext.Abstraction;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Function.RabbitMQ
{
    public class FileExportProcessing
    {
        private readonly ITenantContext _tenantContext;
        private readonly IFileExportService _fileExportService;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContext _userContext;

        public FileExportProcessing(
            ITenantContext tenantContext,
            IFileExportService fileExportService,
            IAuditLogService auditLogService,
            IUserContext userContext)
        {
            _tenantContext = tenantContext;
            _fileExportService = fileExportService;
            _auditLogService = auditLogService;
            _userContext = userContext;
        }

        [FunctionName("FileExportProcessing")]
        public async Task RunAsync(
            [RabbitMQTrigger("asset.table.function.file.exported.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
            ILogger log,
            ExecutionContext context)
        {
            BaseModel<ExportFileMessage> request = data.Deserialize<BaseModel<ExportFileMessage>>();
            var eventMessage = request.Message;

            // setup Domain to use inside repository
            _tenantContext.RetrieveFromString(eventMessage.TenantId, eventMessage.SubscriptionId, eventMessage.ProjectId);
            _userContext.SetApplicationId(eventMessage.ApplicationId);
            var result = await _fileExportService.ExportFileAsync(
                eventMessage.TableId,
                eventMessage.RequestedBy,
                eventMessage.ActivityId,
                context,
                eventMessage.Filter,
                eventMessage.DateTimeFormat,
                eventMessage.Timezone);
            await LogActivityAsync(eventMessage, result, eventMessage.RequestedBy);
        }

        private Task LogActivityAsync(ExportFileMessage message, ImportExportBasePayload payload, string requestedBy)
        {
            var activityMessage = payload.CreateLog(requestedBy, _tenantContext, _auditLogService.AppLevel);
            activityMessage.EntityId = message.TableId.ToString();
            activityMessage.EntityName = message.TableName;
            return _auditLogService.SendLogAsync(activityMessage);
        }
    }
}
