using System;
using System.Collections.Generic;
using AHI.Infrastructure.Service.Dapper.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class GetTableData : IRequest<IEnumerable<object>>
    {
        public Guid Id { get; set; }
        public QueryCriteria QueryCriteria { get; set; }

        public GetTableData(Guid id, QueryCriteria queryCriteria)
        {
            Id = id;

            if (queryCriteria.PageIndex == 0 && queryCriteria.PageSize == 0)
                queryCriteria.PageSize = 20;

            QueryCriteria = queryCriteria;
        }
    }
}