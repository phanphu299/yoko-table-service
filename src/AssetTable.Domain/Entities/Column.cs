using System;
using AHI.Infrastructure.Repository.Model.Generic;

namespace AssetTable.Domain.Entity
{
    public class Column : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public bool AllowNull { get; set; }
        public Guid TableId { get; set; }
        public Table Table { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public int ColumnOrder { get; set; }
        public bool IsSystemColumn { get; set; }
    }
}
