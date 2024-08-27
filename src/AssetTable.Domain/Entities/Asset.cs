using System;
using AHI.Infrastructure.Repository.Model.Generic;

namespace AssetTable.Domain.Entity
{
    public class Asset : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentAssetId { get; set; }
        public Guid? AssetTemplateId { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public int RetentionDays { get; set; }
        public virtual Asset ParentAsset { get; set; }
        public Asset()
        {
            Id = Guid.NewGuid();
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            RetentionDays = 90;
        }
    }
}
