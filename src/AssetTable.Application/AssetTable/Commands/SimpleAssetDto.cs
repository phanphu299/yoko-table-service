using System;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class SimpleAssetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ResourcePath { get; set; }
        public string CreatedBy { get; set; }
    }
}
