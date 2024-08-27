using System.Collections.Generic;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using System;
using AHI.Infrastructure.UserContext.Abstraction;

namespace AssetTable.Application.Event
{
    public class FileImportEvent : BusEvent
    {
        public override string TopicName => "asset.table.application.event.file.imported";
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public IEnumerable<string> FileNames { get; set; }
        public string RequestedBy { get; set; }
        public string DateTimeFormat { get; set; }
        public AHI.Infrastructure.UserContext.Models.TimeZone Timezone { get; set; }
        public string ApplicationId { get; set; }
        public FileImportEvent(Guid tableId, string tableName, IEnumerable<string> fileNames, ITenantContext tenantContext, IUserContext userContext)
        {
            TableId = tableId;
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            FileNames = fileNames;
            RequestedBy = userContext.Upn;
            DateTimeFormat = userContext.DateTimeFormat;
            Timezone = userContext.Timezone;
            TableName = tableName;
            ApplicationId = userContext.ApplicationId;
        }

    }
}
