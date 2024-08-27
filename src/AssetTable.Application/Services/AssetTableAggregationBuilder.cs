using System.Linq;
using AHI.Infrastructure.Service.Dapper.Extensions;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.Service
{
    public class AssetTableAggregationBuilder
    {
        private readonly string[] _validAggregations = { "max", "min", "sum", "avg" };
        private readonly string[] _validOperations = { "=", ">", "<", ">=", "<=" };

        private readonly TableDto _table;
        private readonly string _targetColumnName;
        private readonly AggregationCriteria _aggregationCriteria;
        private string _query;
        private object _value;

        public string TableName => TableService.GetTableName(_table.Id);

        public AssetTableAggregationBuilder(string targetColumnName, AggregationCriteria aggregationCriteria, TableDto table)
        {
            _targetColumnName = targetColumnName;
            _aggregationCriteria = aggregationCriteria;
            _table = table;
        }

        public AssetTableAggregationBuilder BuildFilter()
        {
            if (_validAggregations.Contains(_aggregationCriteria.AggregationType) && _validOperations.Contains(_aggregationCriteria.FilterOperation))
            {
                _query = $"select coalesce({_aggregationCriteria.AggregationType}({_targetColumnName.ToColumnStringName()}), 0) from \"{TableName}\" where {_aggregationCriteria.FilterName.ToColumnStringName()} {_aggregationCriteria.FilterOperation} @FilterValue";
                _value = _aggregationCriteria.FilterValue;
            }
            _value = new
            {
                FilterValue = _value
            };
            return this;
        }

        public (string Qyery, object Value) BuildQuery()
        {
            return (_query, _value);
        }
    }
}