using System;
using AssetTable.Application.Models;
using MediatR;

namespace AssetTable.Application.FileRequest.Command
{
    public class ExportFile : IRequest<ActivityResponse>
    {
        public Guid ActivityId { get; set; } = Guid.NewGuid();
        public Guid TableId { get; set; }
        public string Filter { get; set; }
    }
}
