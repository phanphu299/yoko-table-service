using System;

namespace AHI.AssetTable.Function.Model
{
    public class NotificationMessage
    {
        public string Type { get; set; }
        public string Upn { get; set; }
        public object Payload { get; set; }

        public NotificationMessage(string type, string upn, object payload)
        {
            Type = type;
            Payload = payload;
            Upn = upn;
        }
    }

    public class DeviceNotificationMessage : NotificationMessage
    {
        public string DeviceId { get; set; }
        public DeviceNotificationMessage(string type, string deviceId, object payload) : base(type, null, payload)
        {
            DeviceId = deviceId;
        }
    }
    public class AssetNotificationMessage : NotificationMessage
    {
        public Guid AssetId { get; set; }
        public AssetNotificationMessage(string type, Guid assetId) : base(type, null, null)
        {
            AssetId = assetId;
        }
    }
}
