using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using AssetTable.Application.Services;
using AssetTable.Application.Extension;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.Service
{
    public class UpdateTableScriptBuilder : BaseTableScriptBuidler
    {
        private readonly TableDto _requestTable;
        private readonly IEnumerable<ColumnDto> _requestColumns;

        private readonly TableDto _targetTable;
        private readonly IEnumerable<ColumnDto> _targetColumns;

        private StringBuilder _scriptBuilder;

        private string _tableName => TableService.GetTableName(_targetTable.Id);

        public UpdateTableScriptBuilder(TableDto requestTable, TableDto targetTable)
            : base(requestTable)
        {
            _requestTable = requestTable;
            _requestColumns = requestTable.Columns;

            _targetTable = targetTable;
            _targetColumns = targetTable.Columns;

            _scriptBuilder = new StringBuilder();
        }

        public UpdateTableScriptBuilder Validate()
        {
            base.ValidateTable();
            return this;
        }

        public UpdateTableScriptBuilder BuildTable()
        {
            var scripts = new List<string>();

            if (!_targetTable.HasData)
            {
                var addTableBuilder = new AddTableScriptBuilder(_requestTable);
                scripts.Add(GetDropTableScript());
                scripts.Add(addTableBuilder.BuildTable().GetScript());
            }
            else
            {
                var columnBuilder = new ColumnBuilder(_tableName);
                foreach (var requestColumn in _requestColumns)
                {
                    var targetColumn = _targetColumns.FirstOrDefault(x => x.Id == requestColumn.Id);
                    columnBuilder.SetColumns(requestColumn, targetColumn);
                    scripts.AddRange(columnBuilder.BuildColumn().GetScript());
                }
                scripts.AddRange(columnBuilder.BuildRenaneRule().GetScript());
            }

            _scriptBuilder.Append(string.Join(Environment.NewLine, scripts));

            return this;
        }

        public string GetScript()
        {
            return _scriptBuilder.ToString();
        }

        private string GetDropTableScript()
        {
            return $"drop table if exists {_tableName.ToStringQuote()};";
        }
    }
}