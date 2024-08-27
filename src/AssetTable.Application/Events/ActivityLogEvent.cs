using System;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;

namespace AssetTable.Application.Events
{
    public class ActivityLogEvent : BusEvent
    {
        public override string TopicName => "audit.application.log.created";
        public Guid Id { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public string Entity { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public string RequestedBy { get; set; }
        public string Status { get; set; }
        public string[] Payload { get; set; }

        public ActivityLogEvent(string entity, string entityId, ActivitiesLogEventAction action, ITenantContext tenantContext, string requestedBy, ActivitiesLogEventStatus status, string[] payload = default)
        {
            Id = Guid.NewGuid();
            Entity = entity;
            EntityId = entityId;
            Action = action.ToString();
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            RequestedBy = requestedBy;
            Status = GetStatusString(status);
            Payload = payload;
        }
        public ActivityLogEvent(string entity, string entityId, ActivitiesLogEventAction action, ITenantContext tenantContext, string requestedBy, ActivitiesLogEventStatus status, string payload = default)
            : this(entity, entityId, action, tenantContext, requestedBy, status, new string[] { payload })
        {
        }

        private static string GetStatusString(ActivitiesLogEventStatus status)
        {
            if (status == ActivitiesLogEventStatus.PartialSuccess)
            {
                return "Partial success";
            }
            return status.ToString();
        }
    }

    public enum ActivitiesLogEventAction
    {
        Add_Folder,
        Update_Folder,
        Delete_File,
        Delete_Folder,
        Import,
        Export,
        Upload,
        Upload_File,
        Insert,
        Clone,
        Move_File,
        Generate_Primary_Key,
        Generate_Secondary_Key,
        Send_Data,
        Download_Metrics
    }

    public enum ActivitiesLogEventStatus
    {
        Success,
        Fail,
        PartialSuccess
    }
}
