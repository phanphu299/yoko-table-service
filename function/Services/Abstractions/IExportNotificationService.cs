using System;
using System.Threading.Tasks;
using AHI.Infrastructure.Audit.Constant;
using AHI.Infrastructure.Audit.Model;

namespace AHI.AssetTable.Function.Service.Abstraction
{
    public interface IExportNotificationService
    {
        Guid ActivityId { get; set; }
        string ObjectName { get; set; }
        ActionType NotificationType { get; set; }
        string Upn { get; set; }
        string URL { get; set; }

        Task<ImportExportNotifyPayload> SendFinishExportNotifyAsync(ActionStatus status);
    }
}
