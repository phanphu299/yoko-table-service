using System;
using System.Threading.Tasks;
using AHI.Infrastructure.Audit.Constant;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.UserContext.Abstraction;

namespace AHI.AssetTable.Function.Service
{
    public class ExportNotificationService : Abstraction.IExportNotificationService
    {
        public Guid ActivityId { get; set; }
        public string ObjectName { get; set; }
        public ActionType NotificationType { get; set; }
        public string Upn { get; set; }
        public string URL { get; set; }
        private string NotifyEndpoint = "ntf/notifications/export/notify";
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;

        public ExportNotificationService(INotificationService notificationService, IUserContext userContext)
        {
            _notificationService = notificationService;
            _userContext = userContext;
        }

        public async Task<ImportExportNotifyPayload> SendFinishExportNotifyAsync(ActionStatus status)
        {
            var message = new ExportNotifyPayload(ActivityId, ObjectName, DateTime.UtcNow, NotificationType, status)
            {
                URL = URL,
                Description = GetFinishExportNotifyDescription(status)
            };
            await _notificationService.SendNotifyAsync(NotifyEndpoint, new UpnNotificationMessage(ObjectName,
                                                                                                    Upn,
                                                                                                    _userContext.ApplicationId ?? Constant.ApplicationInformation.APPLICATION_ID,
                                                                                                    message));
            return message;
        }

        private string GetFinishExportNotifyDescription(ActionStatus status)
        {
            return status switch
            {
                ActionStatus.Start => DescriptionMessage.EXPORT_START,
                ActionStatus.Success => DescriptionMessage.EXPORT_SUCCESS,
                ActionStatus.Fail => DescriptionMessage.EXPORT_FAIL,
                _ => string.Empty
            };
        }
    }
}