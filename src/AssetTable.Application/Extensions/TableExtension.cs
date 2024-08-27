using System;
using System.Globalization;
using AssetTable.Application.Constant;

namespace AssetTable.Application.Extension
{
    public static class TableExtension
    {
        public static string ToStringQuote(this string text)
        {
            return $"\"{text}\"";
        }

        public static bool IsAddAction(this string columnAction)
        {
            return string.Equals(columnAction, ColumnAction.ADD, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsUpdateAction(this string columnAction)
        {
            return string.Equals(columnAction, ColumnAction.UPDATE, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsDeleteAction(this string columnAction)
        {
            return string.Equals(columnAction, ColumnAction.DELETE, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsNoAction(this string columnAction)
        {
            return string.Equals(columnAction, ColumnAction.NO, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsTypeDateTime(this string columnType)
        {
            return string.Equals(columnType, DataTypeConstants.TYPE_DATETIME, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsTypeBool(this string columnType)
        {
            return string.Equals(columnType, DataTypeConstants.TYPE_BOOLEAN, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsTypeDouble(this string columnType)
        {
            return string.Equals(columnType, DataTypeConstants.TYPE_DOUBLE, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsTypeInt(this string columnType)
        {
            return string.Equals(columnType, DataTypeConstants.TYPE_INTEGER, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsTypeText(this string columnType)
        {
            return string.Equals(columnType, DataTypeConstants.TYPE_TEXT, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsTypeTimestamp(this string columnType)
        {
            return string.Equals(columnType, DataTypeConstants.TYPE_TIMESTAMP, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsSpecialType(this string columnType)
        {
            return IsTypeBool(columnType) || IsTypeDateTime(columnType);
        }

        public static bool IsValidName(this string name)
        {
            return PostgresEntityName.CheckValidName(name);
        }

        public static bool IsValidTypeCode(this string typeCode)
        {
            return PostgresDataTypeMapping.CheckDataType(typeCode);
        }

        public static bool IsValidDefaultValue(this string typeCode, string defaultValue)
        {
            return PostgresDataTypeMapping.CheckDefaultValue(typeCode, defaultValue);
        }

        public static string GetDataType(this string text)
        {
            return PostgresDataTypeMapping.GetDataType(text);
        }
        public static string GenerateAutoIncrement(this string text)
        {
            return PostgresDataTypeMapping.GenerateAutoIncrement(text);
        }

        public static string GetColumnType(this bool allowNull)
        {
            return PostgresColumnTypeMapping.GetColumnType(allowNull);
        }

        public static string GetColumnState(this bool allowNull)
        {
            return PostgresColumnTypeMapping.GetColumnState(allowNull);
        }

        public static string GetDefaultValue(this string typeCode, string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue))
                return $"default null";
            // for datetime, the default value should be converted into proper form
            if (typeCode == PostgresDataTypeMapping.DATETIME)
            {
                var datetime = DateTime.ParseExact(defaultValue, AHI.Infrastructure.SharedKernel.Extension.Constant.DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                // 2002-04-20 17:31:12.66
                var postgresqlDatimeString = datetime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                return $"default '{postgresqlDatimeString}'";
            }
            return $@"default {(PostgresDataTypeMapping.CheckDefaultValueNeedConvert(typeCode) ? $"'{defaultValue.Replace("{", "{{").Replace("}", "}}").Replace("'", "''")}'::character varying"
                : $"'{defaultValue}'")}";
        }
    }
}