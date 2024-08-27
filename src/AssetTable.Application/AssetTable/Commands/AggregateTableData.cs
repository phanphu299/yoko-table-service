using System;
using AssetTable.Application.Service;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class AggregateTableData : IRequest<object>
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public AggregationCriteria AggregationCriteria { get; set; }

        public AggregateTableData(Guid id, string columnName, AggregationCriteria aggregationCriteria = null)
        {
            Id = id;
            ColumnName = columnName;
            AggregationCriteria = aggregationCriteria;
        }
    }
}
