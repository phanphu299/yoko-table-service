using System;
using AssetTable.Application.Constant;

namespace AssetTable.Application.Model
{
    public class NotificationMessage
    {
        public string Type { get; set; }
        public object Payload { get; set; }
        public Guid? AssetId { get; set; }
        public Guid TargetId { get; set; }
        public NotificationMessage(Guid targetId, object payload)
        {
            TargetId = targetId;
            Payload = payload;
            Type = NotificationType.LOCK_ENTITY;
        }
        public NotificationMessage(Guid targetId, string type, object payload)
        {
            TargetId = targetId;
            Payload = payload;
            Type = type;
        }
    }
}
