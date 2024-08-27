using System.Linq;
using System.Collections.Generic;
using AssetTable.Application.Extension;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.Services
{
    public class ColumnBuilder
    {
        private readonly string _tableName;
        private ColumnDto _requestColumn;
        private ColumnDto _targetColumn;
        private List<string> _scripts;
        private List<ColumnDto> _rollBackRenameColumns;

        public ColumnBuilder(string tableName)
        {
            _tableName = tableName;
            _scripts = new List<string>();
            _rollBackRenameColumns = new List<ColumnDto>();
        }

        public void SetColumns(ColumnDto requestColumn, ColumnDto targetColumn)
        {
            _requestColumn = requestColumn;
            _targetColumn = targetColumn;
        }

        public ColumnBuilder BuildColumn()
        {
            // Column data type must be reserved in case the table has data
            if (_requestColumn != null && _targetColumn != null && !_requestColumn.EqualsTypeCode(_targetColumn))
                return this;

            // Rule: you cannot delete or alter the primary key
            if (_requestColumn.HasDeleted() && _targetColumn != null && !_targetColumn.IsPrimary)
                GetDeleteColumnScript();

            if (_requestColumn.HasAdded() && _targetColumn == null)
                GetAddColumnScript();

            if (_requestColumn.HasUpdated() && _targetColumn != null && !_targetColumn.IsPrimary)
                BuildUpdateColumnScript();

            return this;
        }

        /// <summary>
        /// The function will rollback all column names to their requested names as they've changed to temp names
        /// </summary>
        /// <returns></returns>
        public ColumnBuilder BuildRenaneRule()
        {
            _scripts.AddRange(_rollBackRenameColumns.Select(col => GetRenameColumnScript(col.Name, col.TempName)));
            return this;
        }

        public IEnumerable<string> GetScript()
        {
            var scripts = _scripts;
            Reset();
            return scripts;
        }

        private void Reset()
        {
            _requestColumn = new ColumnDto();
            _targetColumn = new ColumnDto();
            _scripts = new List<string>();
        }

        private ColumnBuilder BuildUpdateColumnScript()
        {
            if (!_requestColumn.EqualsAllowNull(_targetColumn))
                GetAllowNullRuleScript();

            if (!_requestColumn.EqualsDefaultValue(_targetColumn))
                GetDropDefaultValueScript().GetAlterDefaultValueScript();

            // Rename is the last step do to cause needed to handle swap columns logic
            if (!_requestColumn.EqualsName(_targetColumn))
                GetRenameColumnScript();

            return this;
        }

        /// <summary>
        /// The logic to handle swapping between 2 columns
        //  Table:
        //      first_name | last_name (1)
        //  Expect:
        //      last_name | first_name (2)
        //  Solution:
        //     Step 1: first_name | last_name (1)
        //     Step 2: last_name_temp | last_name
        //     Step 3: last_name_temp | first_name_temp
        //     Step 4: last_name | first_name (2)
        /// </summary>
        /// <returns></returns>
        private ColumnBuilder GetRenameColumnScript()
        {
            var query = GetRenameColumnScript(_requestColumn.TempName, _targetColumn.Name);
            _scripts.Add(query);
            _rollBackRenameColumns.Add(_requestColumn);
            return this;
        }

        private string GetRenameColumnScript(string requestColumnName, string targetColumnName)
        {
            return $"alter table if exists {_tableName.ToStringQuote()} rename {targetColumnName.ToStringQuote()} to {requestColumnName.ToStringQuote()};";
        }

        private ColumnBuilder GetAlterDefaultValueScript()
        {
            var query = $"alter table if exists {_tableName.ToStringQuote()} alter column {_targetColumn.Name.ToStringQuote()} set {_requestColumn.TypeCode.GetDefaultValue(_requestColumn.DefaultValue)};";
            _scripts.Add(query);
            return this;
        }

        private ColumnBuilder GetDropDefaultValueScript()
        {
            var query = $"alter table if exists {_tableName.ToStringQuote()} alter column {_targetColumn.Name.ToStringQuote()} drop default;";
            _scripts.Add(query);
            return this;
        }

        private ColumnBuilder GetAllowNullRuleScript()
        {
            var query = $"alter table if exists {_tableName.ToStringQuote()} alter column {_targetColumn.Name.ToStringQuote()} {_requestColumn.AllowNull.GetColumnState()};";
            _scripts.Add(query);
            return this;
        }

        private ColumnBuilder GetDeleteColumnScript()
        {
            var query = $"alter table if exists {_tableName.ToStringQuote()} drop column if exists {_targetColumn.Name.ToStringQuote()};";
            _scripts.Add(query);
            return this;
        }

        private ColumnBuilder GetAddColumnScript()
        {
            var query = $"alter table if exists {_tableName.ToStringQuote()} add column {_requestColumn.Name.ToStringQuote()} {_requestColumn.TypeCode.GetDataType()} {_requestColumn.AllowNull.GetColumnType()} {_requestColumn.TypeCode.GetDefaultValue(_requestColumn.DefaultValue)};";
            _scripts.Add(query);
            return this;
        }
    }
}