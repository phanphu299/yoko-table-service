using AssetTable.Application.Constant;
using AssetTable.Application.AssetTable.Command.Model;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using System;

namespace AssetTable.Application.AssetTable.Command
{
    public class GetListTable : BaseCriteria, IRequest<BaseSearchResponse<GetListTableDto>>
    {
        public Guid AssetId { get; set; }

        public GetListTable(Guid assetId)
        {
            AssetId = assetId;
            PageIndex = 0;
            PageSize = int.MaxValue;
            Sorts = DefaultSearchConstants.DEFAULT_SORT;
        }
    }
}
