using System;
using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class UpsertTableData : IRequest<BaseResponse>
    {
        public Guid Id { get; set; }

        public string DefaultColumnAction { get; set; }

        public IEnumerable<IDictionary<string, object>> Data { get; set; }

        public string CallSource { get; set; }

        public UpsertTableData(Guid id, IEnumerable<IDictionary<string, object>> data, string defaultColumnAction = null, string callSource = null)
        {
            Id = id;
            Data = data;
            DefaultColumnAction = defaultColumnAction;
            CallSource = callSource;
        }
    }
}