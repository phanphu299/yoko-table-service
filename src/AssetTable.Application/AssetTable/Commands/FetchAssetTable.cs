using System;
using AssetTable.Application.AssetTable.Command.Model;
using MediatR;

namespace AssetTable.Application.Service
{
    public class FetchAssetTable : IRequest<TableDto>
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }

        public FetchAssetTable(Guid id, Guid assetId)
        {
            Id = id;
            AssetId = assetId;
        }
    }
}
