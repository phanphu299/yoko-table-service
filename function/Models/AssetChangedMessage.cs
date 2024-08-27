using System;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;

namespace AHI.AssetTable.Function.Model
{
    public class AssetChangedMessage : BusEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TotalAsset { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public bool DeleteTable { get; set; }
        public string ResourcePath { get; set; }

        public override string TopicName => "";
    }
}
