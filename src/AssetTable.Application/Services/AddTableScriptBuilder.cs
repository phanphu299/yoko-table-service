using System.Linq;
using System.Collections.Generic;
using AssetTable.Application.Extension;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Constant;

namespace AssetTable.Application.Service
{
    public class AddTableScriptBuilder : BaseTableScriptBuidler
    {
        private readonly TableDto _table;
        private readonly ColumnDto _primaryColumn;
        private readonly IEnumerable<ColumnDto> _columns;
        private string _tableScript;
        private IEnumerable<string> _columnScripts;

        private string _tableName => TableService.GetTableName(_table.Id);

        public AddTableScriptBuilder(TableDto table)
            : base(table)
        {
            _table = table;
            _primaryColumn = table.Columns.First(x => x.IsPrimary);
            _columns = table.Columns.Where(x => !x.IsPrimary);
            _columnScripts = new List<string>();
        }

        public AddTableScriptBuilder Validate()
        {
            base.ValidateTable();
            return this;
        }

        public AddTableScriptBuilder BuildTable()
        {
            _tableScript = GetTableScript();

            BuildColumns();

            return this;
        }

        public string GetScript()
        {
            var query = $"{_tableScript} ({string.Join(",", _columnScripts)});";
            return query;
        }

        private void BuildColumns()
        {
            var columnScripts = new List<string>();

            columnScripts.Add(GetColumnScript(_primaryColumn));
            columnScripts.AddRange(_columns.Where(col => col.Action != ColumnAction.DELETE).Select(col => GetColumnScript(col)).ToList());

            _columnScripts = columnScripts;
        }

        private string GetTableScript()
        {
            return $"create table if not exists {_tableName.ToStringQuote()}";
        }

        private string GetColumnScript(ColumnDto column)
        {
            var query = string.Empty;
            if (column.IsPrimary)
            {
                query = $"{_primaryColumn.Name.ToStringQuote()} {_primaryColumn.TypeCode.GetDataType()} {_primaryColumn.TypeCode.GenerateAutoIncrement()} primary key";
            }
            else
            {
                query = $"{column.Name.ToStringQuote()} {column.TypeCode.GetDataType()} {column.AllowNull.GetColumnType()}";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    query += $" {column.TypeCode.GetDefaultValue(column.DefaultValue)}";
                }
            }
            return query;
        }
    }
}