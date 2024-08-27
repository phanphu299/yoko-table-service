using System;
using System.Collections.Generic;
using AHI.Infrastructure.Service.Dapper.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class GetAssetTableData : IRequest<IEnumerable<object>>
    {
        public Guid Id { get; set; }
        public QueryCriteria QueryCriteria { get; set; }

        public GetAssetTableData(Guid id, QueryCriteria queryCriteria = null)
        {
            Id = id;
            QueryCriteria = queryCriteria;
        }
    }
}