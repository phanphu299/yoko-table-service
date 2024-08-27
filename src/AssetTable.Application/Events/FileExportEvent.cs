using System;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.UserContext.Abstraction;

namespace AssetTable.Application.Event
{
    public class FileExportEvent : BusEvent
    {
        public override string TopicName => "asset.table.application.event.file.exported";
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
        public FileExportEvent(
            Guid activityId,
            Guid tableId,
            string tableName,
            string filter,
            ITenantContext tenantContext,
            IUserContext userContext)
        {
            ActivityId = activityId;
            TableId = tableId;
            TableName = tableName;
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            Filter = filter;
            RequestedBy = userContext.Upn;
            DateTimeFormat = userContext.DateTimeFormat;
            Timezone = userContext.Timezone;
            ApplicationId = userContext.ApplicationId;
        }
    }
}
