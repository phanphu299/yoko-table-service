using System;
using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class UpsertAssetTableData : IRequest<BaseResponse>
    {
        public Guid AssetId { get; set; }
        public Guid Id { get; set; }
        public string DefaultColumnAction { get; set; }
        public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public bool TrackActivity { get; set; }
        public bool IsUpsert { get; set; }
        public string CallSource { get; set; }

        public UpsertAssetTableData(Guid assetId, Guid id, IEnumerable<IDictionary<string, object>> data, bool trackActivity, string defaultColumnAction = null, bool isUpsert = true, string callSource = null)
        {
            AssetId = assetId;
            Id = id;
            Data = data;
            TrackActivity = trackActivity;
            DefaultColumnAction = defaultColumnAction;
            IsUpsert = isUpsert;
            CallSource = callSource;
        }
    }
}
