using System;

namespace AHI.AssetTable.Function.Model
{
    public class ExportFileMessage
    {
        public Guid ActivityId { get; set; } = Guid.NewGuid();
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public string Filter { get; set; }
        public string RequestedBy { get; set; }
        public string DateTimeFormat { get; set; }
        public AHI.Infrastructure.UserContext.Models.TimeZone Timezone { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public string ApplicationId { get; set; }
    }
}
