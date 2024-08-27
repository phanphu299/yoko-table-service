using System;
using AHI.AssetTable.Function.Model;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Function.Service.Abstraction;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.UserContext.Abstraction;

namespace Function.RabbitMQ
{
    public class FileImportProcessing
    {
        private readonly ITenantContext _tenantContext;
        private readonly IFileImportService _importService;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContext _userContext;
        public FileImportProcessing(ITenantContext tenantContext, IFileImportService importService, IUserContext userContext, IAuditLogService auditLogService)
        {
            _tenantContext = tenantContext;
            _importService = importService;
            _auditLogService = auditLogService;
            _userContext = userContext;
        }

        [FunctionName("FileImportProcessing")]
        public async Task RunAsync(
        [RabbitMQTrigger("asset.table.function.file.imported.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger log, ExecutionContext context)
        {
            BaseModel<ImportFileMessage> request = data.Deserialize<BaseModel<ImportFileMessage>>();
            var activityId = Guid.NewGuid();
            var eventMessage = request.Message;
            // setup Domain to use inside repository
            _tenantContext.RetrieveFromString(eventMessage.TenantId, eventMessage.SubscriptionId, eventMessage.ProjectId);
            _userContext.SetUpn(eventMessage.RequestedBy);
            _userContext.SetApplicationId(eventMessage.ApplicationId);

            var result = await _importService.ImportFileAsync(eventMessage.RequestedBy, activityId, context.FunctionAppDirectory, eventMessage.TableId, eventMessage.FileNames, eventMessage.DateTimeFormat, eventMessage.Timezone);
            await LogActivityAsync(result, eventMessage, eventMessage.RequestedBy);
        }

        private Task LogActivityAsync(ImportExportBasePayload message, ImportFileMessage eventMessage, string requestedBy)
        {
            var activityMessage = message.CreateLog(requestedBy, _tenantContext, _auditLogService.AppLevel);
            activityMessage.EntityId = eventMessage.TableId.ToString();
            activityMessage.EntityName = eventMessage.TableName;
            return _auditLogService.SendLogAsync(activityMessage);
        }
    }
}
