using System;

namespace AHI.AssetTable.Function.Model
{
    public class TableDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
        public string Script { get; set; }
        public Guid? AssetId { get; set; }
        public string AssetName { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { get; set; }
    }
}