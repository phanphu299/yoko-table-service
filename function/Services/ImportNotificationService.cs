using System;
using System.Threading.Tasks;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.Audit.Constant;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.UserContext.Abstraction;

namespace AHI.AssetTable.Function.Service
{
    public class ImportNotificationService : IImportNotificationService
    {
        public Guid ActivityId { get; set; }
        public string ObjectType { get; set; }
        public ActionType NotificationType { get; set; }
        public string Upn { get; set; }
        private readonly string NotifyEndpoint = "ntf/notifications/import/notify";
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;

        public ImportNotificationService(INotificationService notificationService, IUserContext userContext)
        {
            _notificationService = notificationService;
            _userContext = userContext;
        }

        public Task SendStartNotifyAsync(int count)
        {
            var message = new ImportNotifyPayload(ActivityId, ObjectType, DateTime.UtcNow, NotificationType, ActionStatus.Start)
            {
                Description = DescriptionMessage.IMPORT_START,
                Total = count
            };
            return _notificationService.SendNotifyAsync(NotifyEndpoint, new UpnNotificationMessage(ObjectType,
                                                                                                    Upn,
                                                                                                    _userContext.ApplicationId ?? Constant.ApplicationInformation.APPLICATION_ID,
                                                                                                    message));
        }

        public async Task<ImportExportNotifyPayload> SendFinishImportNotifyAsync(ActionStatus status, (int Success, int Fail) partialInfo)
        {
            var message = new ImportNotifyPayload(ActivityId, ObjectType, DateTime.UtcNow, NotificationType, status)
            {
                Description = GetFinishImportNotifyDescription(status),
                Success = partialInfo.Success,
                Fail = partialInfo.Fail

            };
            InitializeMessage(message);
            await _notificationService.SendNotifyAsync(NotifyEndpoint, new UpnNotificationMessage(ObjectType,
                                                                                                    Upn,
                                                                                                    _userContext.ApplicationId ?? Constant.ApplicationInformation.APPLICATION_ID,
                                                                                                    message));
            return message;
        }

        private static string GetFinishImportNotifyDescription(ActionStatus status)
        {
            return status switch
            {
                ActionStatus.Success => DescriptionMessage.IMPORT_SUCCESS,
                ActionStatus.Fail => DescriptionMessage.IMPORT_FAIL,
                ActionStatus.Partial => DescriptionMessage.IMPORT_PARTIAL,
                _ => string.Empty
            };
        }

        private void InitializeMessage(ImportNotifyPayload message)
        {
            message.ActivityId = this.ActivityId;
            message.CreatedUtc = DateTime.UtcNow;
        }
    }
}