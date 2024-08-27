using System.Text;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.AssetTable.Command;
using AssetTable.Application.Constant;
using AssetTable.Application.Extension;
using System;

namespace AssetTable.Application.Service
{
    public class TableDataScriptBuilder
    {
        private readonly AssetTableDto _table;
        private readonly AssetColumnDto _primaryColumn;
        private readonly IEnumerable<AssetColumnDto> _columns;
        private readonly IEnumerable<AssetColumnDto> _systemColumns;
        private readonly IEnumerable<IDictionary<string, object>> _data;
        private readonly string _defaultColumnAction;
        private List<QueryResult> _queryResults;
        private List<string> _errors;

        private string _tableName => TableService.GetTableName(_table.TableId);
        private bool _isUpsert;

        public TableDataScriptBuilder(UpsertAssetTableData command, AssetTableDto table)
        {
            _table = table;
            _columns = table.Columns.Where(x => !x.IsSystemColumn);
            _systemColumns = table.Columns.Where(x => x.IsSystemColumn);
            _primaryColumn = _columns.First(x => x.ColumnIsPrimary);
            _data = command.Data;
            _defaultColumnAction = command.DefaultColumnAction;
            _queryResults = new List<QueryResult>();
            _errors = new List<string>();
            _isUpsert = command.IsUpsert;
        }

        public TableDataScriptBuilder BuildRows(string createdBy)
        {
            foreach (var row in _data)
            {
                if (!row.ContainsKey(ColumnKey.ACTION))
                {
                    row.Add(ColumnKey.ACTION, _defaultColumnAction ?? ColumnAction.NO);
                }

                switch (row[ColumnKey.ACTION])
                {
                    case ColumnAction.ADD:
                    case ColumnAction.UPDATE:
                        var upsertQueryResult = BuildUpsertQuery(row, row[ColumnKey.ACTION] as string, createdBy);

                        if (_errors.Any())
                            break;

                        if (upsertQueryResult != null)
                            _queryResults.Add(upsertQueryResult);

                        break;
                    case ColumnAction.DELETE:
                        var deleteQueryResult = BuildDeleteQuery(row);

                        if (_errors.Any())
                            break;

                        if (deleteQueryResult != null)
                            _queryResults.Add(deleteQueryResult);

                        break;
                    default:
                        break;
                }

                if (_errors.Any())
                    break;
            }

            return this;
        }

        private QueryResult BuildUpsertQuery(IDictionary<string, object> row, string action, string createdBy)
        {
            var queryBuilder = new StringBuilder();
            var value = new ExpandoObject();
            var columns = GetColumns(row);

            var hasInvalidValue = ValidateUpsertColumnValues(columns);
            if (hasInvalidValue)
                return null;

            foreach (var col in columns)
            {
                value.TryAdd(col.Name, col.Value);
            }
            value.TryAdd(SystemColumn.CREATED_BY, createdBy);
            value.TryAdd(SystemColumn.CREATED_UTC, DateTime.UtcNow);
            value.TryAdd(SystemColumn.UPDATED_UTC, DateTime.UtcNow);

            var rowParam = BuildRowParams(columns);
            if (row.ContainsKey(_primaryColumn.ColumnName))
            {
                queryBuilder.Append($@"insert into {rowParam.TableName}({rowParam.ColumnNames}) values({rowParam.InsertTokens})");

                if (!string.IsNullOrEmpty(rowParam.UpdateTokens))
                {
                    if (action == ColumnAction.UPDATE || _isUpsert == true)
                    {
                        queryBuilder.Append(@$" on conflict ({rowParam.PrimaryColumnName}) do update
                                            set {rowParam.UpdateTokens};");
                    }
                }
                else
                {
                    queryBuilder.Append(";");
                }
            }
            else if (!row.ContainsKey(_primaryColumn.ColumnName) && PostgresDataTypeMapping.IsNumbericTypeCode(_primaryColumn.ColumnTypeCode))
            {
                var columnNames = rowParam.ColumnNames.Any() ? $"{rowParam.PrimaryColumnName},{rowParam.ColumnNames}" : $"{rowParam.PrimaryColumnName}";
                var columnValues = rowParam.ColumnNames.Any() ? $"{GetMaxId()}, {rowParam.InsertTokens}" : $"{GetMaxId()}";
                queryBuilder.Append($"insert into {rowParam.TableName}({columnNames}) values({columnValues});");
            }

            var query = queryBuilder.ToString();
            var queryResult = new QueryResult(query, value);

            return queryResult;
        }

        private QueryResult BuildDeleteQuery(IDictionary<string, object> row)
        {
            var query = $"delete from {_tableName.ToStringQuote()} where {_primaryColumn.ColumnName.ToStringQuote()} = @{_primaryColumn.ColumnName};";
            var columns = GetColumns(row);

            var hasInValidValue = ValidateDeleteColumnValues(columns);
            if (hasInValidValue)
                return null;

            var column = columns.FirstOrDefault(x => x.IsPrimary);

            if (column == null)
                return null;

            var value = new ExpandoObject();
            value.TryAdd(_primaryColumn.ColumnName, column.Value);
            var queryResult = new QueryResult(query, value);

            return queryResult;
        }

        private IEnumerable<ColumnInfo> GetColumns(IDictionary<string, object> row)
        {
            var columns = _columns.Join(row,
                a => a.ColumnName,
                b => b.Key,
                (a, b) =>
                {
                    var result = PostgresDataTypeMapping.GetValue(b.Value, a.ColumnTypeCode, a.ColumnAllowNull);
                    return new ColumnInfo(a.ColumnName, a.ColumnTypeCode, a.ColumnIsPrimary, result.Value, result.Success);
                }).ToList();
            return columns;
        }

        private bool ValidateUpsertColumnValues(IEnumerable<ColumnInfo> columns)
        {
            var invalidValueErrors = columns.Where(x => !x.ParsedSuccess).Select(x => $"Column '{x.Name}' has invalid value");

            if (invalidValueErrors.Any())
                _errors.AddRange(invalidValueErrors);

            var requiredColumnNames = _columns.Where(x => !x.ColumnIsPrimary && !x.ColumnAllowNull && string.IsNullOrEmpty(x.ColumnDefaultValue)).Select(x => x.ColumnName);
            var requestedColumnNames = columns.Select(x => x.Name);
            var requiredColumnErrors = requiredColumnNames.Where(columnName => !requestedColumnNames.Contains(columnName))
                                                            .Select(columnName => $"Column '{columnName}' is required");
            if (requiredColumnErrors.Any())
                _errors.AddRange(requiredColumnErrors);

            return _errors.Any();
        }

        private bool ValidateDeleteColumnValues(IEnumerable<ColumnInfo> columns)
        {
            var invalidValueErrors = columns.Where(x => x.IsPrimary && !x.ParsedSuccess).Select(x => $"Column '{x.Name}' has invalid value");
            if (invalidValueErrors.Any())
                _errors.AddRange(invalidValueErrors);
            return _errors.Any();
        }

        private (string TableName, string PrimaryColumnName, string ColumnNames, string InsertTokens, string UpdateTokens) BuildRowParams(IEnumerable<ColumnInfo> columns)
        {
            var tableName = _tableName.ToStringQuote();
            var primaryColumnName = _primaryColumn.ColumnName.ToStringQuote();

            var allColumns = columns.Select(x => x.Name).Concat(_systemColumns.Select(x => x.ColumnName));
            var updateColumns = columns.Select(x => x.Name).Append(SystemColumn.UPDATED_UTC);

            var columnNames = string.Join(",", allColumns.Select(x => x.ToStringQuote()));
            var insertTokens = string.Join(",", allColumns.Select(x => $"@{x}"));
            var updateTokens = string.Join(",", updateColumns.Where(x => x != _primaryColumn.ColumnName).Select(x => $"{x.ToStringQuote()} = {GetExcludedStringName(x.ToStringQuote())}"));
            return (tableName, primaryColumnName, columnNames, insertTokens, updateTokens);
        }

        private string GetMaxId()
        {
            return $"coalesce((select max({_primaryColumn.ColumnName.ToStringQuote()}) + 1 from {_tableName.ToStringQuote()}), 1)";
        }

        private string GetExcludedStringName(string columnName)
        {
            return $"excluded.{columnName}";
        }

        public (IEnumerable<string> Errors, IEnumerable<QueryResult> QueryResults) BuildScript()
        {
            return (_errors, _queryResults);
        }
    }

    public class ColumnInfo
    {
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public bool IsPrimary { get; set; }
        public object Value { get; set; }
        public bool ParsedSuccess { get; set; }
        public bool HasRequiredValue { get; set; }

        public ColumnInfo(string name, string typeCode, bool isPrimary, object value, bool parsedSuccess)
        {
            Name = name;
            TypeCode = typeCode;
            IsPrimary = isPrimary;
            Value = value;
            ParsedSuccess = parsedSuccess;
        }
    }

    public class QueryResult
    {
        public string Query { get; set; }
        public ExpandoObject Value { get; set; }

        public QueryResult(string query, ExpandoObject value)
        {
            Query = query;
            Value = value;
        }
    }
}
