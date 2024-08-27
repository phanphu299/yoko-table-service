using System;
using AssetTable.Application.AssetTable.Command.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class ArchiveAssetTable : IRequest<ArchiveAssetTableDto>
    {
        public DateTime ArchiveTime { get; set; } = DateTime.UtcNow;
    }
}
