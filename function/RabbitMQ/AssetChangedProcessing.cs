using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AHI.AssetTable.Function.Model;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace AHI.AssetTable.Function.Trigger.RabbitMQ
{
    public class AssetChangedProcessing
    {
        private readonly ITenantContext _tenantContext;
        private readonly ITableService _tableService;
        public AssetChangedProcessing(ITenantContext tenantContext, ITableService tableService)
        {
            _tenantContext = tenantContext;
            _tableService = tableService;
        }

        [FunctionName("AssetChangedProcessing")]
        public async Task RunAsync(
        [RabbitMQTrigger("assettable.function.asset.changed.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger log, ExecutionContext context)
        {
            BaseModel<AssetChangedMessage> request = data.Deserialize<BaseModel<AssetChangedMessage>>();
            var eventMessage = request.Message;

            if (eventMessage.ActionType == Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Updated)
            {
                // setup Domain to use inside repository
                _tenantContext.RetrieveFromString(eventMessage.TenantId, eventMessage.SubscriptionId, eventMessage.ProjectId);
                await _tableService.UpdateAssetAsync(eventMessage.Id, eventMessage.Name, eventMessage.ResourcePath);
            }
            if (eventMessage.ActionType == Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Deleted)
            {
                // setup Domain to use inside repository
                _tenantContext.RetrieveFromString(eventMessage.TenantId, eventMessage.SubscriptionId, eventMessage.ProjectId);
                await _tableService.DeleteTableAsync(eventMessage.Id, eventMessage.DeleteTable);
            }
        }
    }
}
