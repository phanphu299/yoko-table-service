using System;
using System.Collections.Generic;
using AHI.Infrastructure.Repository.Model.Generic;

namespace AssetTable.Domain.Entity
{
    public class Table : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
        public string Description { get; set; }
        public string Script { get; set; }
        public Guid? AssetId { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual ICollection<Column> Columns { get; set; }
        public string AssetName { get; set; }
        public string AssetCreatedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ResourcePath { get; set; }
        public virtual ICollection<EntityTagDb> EntityTags { get; set; }

        public Table()
        {
            Columns = new List<Column>();
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            EntityTags ??= new List<EntityTagDb>();
        }

        public Table(IEnumerable<Column> entities)
            : this()
        {
            foreach (var entity in entities)
            {
                Columns.Add(entity);
            }
        }
    }
}
