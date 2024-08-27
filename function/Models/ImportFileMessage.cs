using System;
using System.Collections.Generic;

namespace AHI.AssetTable.Function.Model
{
    public class ImportFileMessage
    {
        public Guid TableId { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public IEnumerable<string> FileNames { get; set; }
        public string RequestedBy { get; set; }
        public string DateTimeFormat { get; set; }
        public string TableName { get; set; }
        public Infrastructure.UserContext.Models.TimeZone Timezone { get; set; }
        public string ApplicationId { get; set; }
    }
}