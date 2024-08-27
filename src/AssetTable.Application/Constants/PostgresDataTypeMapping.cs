using System.Collections.Generic;
using System;
using AssetTable.ApplicationExtension.Extension;

namespace AssetTable.Application.Constant
{
    public static class PostgresDataTypeMapping
    {
        public static string BIGIN = "BIGIN";
        public static string BOOL = "BOOL";
        public static string INT = "INT";
        public static string REAL = "REAL";
        public static string DOUBLE = "DOUBLE";
        public static string DECIMAL = "DECIMAL";
        public static string TEXT = "TEXT";
        public static string TIMESTAMP = "TIMESTAMP";
        public static string DATETIME = "DATETIME";
        public static string VA25 = "VA25";
        public static string VA255 = "VA255";
        public static string VA50 = "VA50";

        private static readonly IDictionary<string, string> _dataTypeMapping = new Dictionary<string, string>() {
            { BIGIN, "bigint" },
            { BOOL, "boolean" },
            { INT, "int" },
            { REAL, "real" },
            { DOUBLE,"double precision"},
            { DECIMAL,"double precision"},
            { TEXT, "text" },
            { TIMESTAMP, "bigint" },
            { DATETIME, "timestamp without time zone"},
            { VA25, "varchar(25)" },
            { VA255, "varchar(255)" },
            { VA50, "varchar(50)" }
        };

        private static readonly List<string> _defaultValueTypeNeedConvert = new List<string>() {
            VA25,
            VA255,
            VA50
        };

        private static readonly IDictionary<string, Func<string, bool>> _defaultValueMapping = new Dictionary<string, Func<string, bool>>() {
            { BIGIN, (text) => text.IsNumber() },
            { BOOL, (text) => text.IsBoolean() },
            { INT, (text) => text.IsNumber() },
            { REAL, (text) => text.IsDecimalNumber() },
            { DOUBLE, (text) => text.IsDoubleNumber() },
            { DECIMAL, (text) => text.IsDecimalNumber() },
            { TEXT, (text) => true },
            { TIMESTAMP, (text) => text.IsTimestamp() },
            { DATETIME, (text) => text.IsDateTime() },
            { VA25, (text) => text.IsLessThanOrEqualsTo(25) },
            { VA255, (text) => text.IsLessThanOrEqualsTo(255) },
            { VA50, (text) => text.IsLessThanOrEqualsTo(50) }
        };

        private static readonly IDictionary<string, Func<object, bool, (bool Success, object Result)>> _valueParser = new Dictionary<string, Func<object, bool, (bool Success, object Result)>>() {
            /* considering this case
                Scenario: allowNull = true but the value is '' -> this will case the issue.
                Solution: for numberic/datetime type -> if the value is null or '' => target value will be null
                Az issue: https://dev.azure.com/AssetHealthInsights/Asset%20Backlogs/_workitems/edit/57061
            */
            
            { BIGIN, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    long result;
                    var success = value.ConvertToLongNumber(out result);
                    return (success, result);
                }
            },
            { BOOL, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    bool result;
                    var success = value.ConvertToBoolean(out result);
                    return (success, result);
                }
            },
            { INT, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    int result;
                    var success = value.ConvertToIntNumber(out result);
                    return (success, result);
                }
            },
            { REAL, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    double result;
                    var success = value.ConvertToDoubleNumber(out result);
                    return (success, result);
                }
            },
            { DOUBLE, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    double result;
                    var success = value.ConvertToDoubleNumber(out result);
                    return (success, result);
                }
            },
            { DECIMAL, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    decimal result;
                    var success = value.ConvertToDecimalNumber(out result);
                    return (success, result);
                }
            },
            { TIMESTAMP, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    double result;
                    var success = value.ConvertToDoubleNumber(out result);
                    return (success, result);
                }
            },
            { DATETIME, (value, allowNull) => {
                    if (string.IsNullOrEmpty(value?.ToString()) && allowNull)
                        return (true, null);

                    DateTime result;
                    var success = value.ConvertToDateTime(out result);
                    return (success, result);
                }
            },
            { TEXT, (value, allowNull) => (true, value) },
            { VA25, (value, allowNull) => (value.IsLessThanOrEqualsTo(25), value) },
            { VA255, (value, allowNull) => (value.IsLessThanOrEqualsTo(255), value) },
            { VA50, (value, allowNull) => (value.IsLessThanOrEqualsTo(50), value) }
        };

        private static readonly List<string> _numbericTypeCodes = new List<string>() {
            INT,
            BIGIN,
            REAL,
            DOUBLE,
            DECIMAL
        };

        /*
        If you really want to insert into identity column, you can use GENERATED BY DEFAULT 
        instead of GENERATED ALWAYS. 
        In that case if you haven't provided value for identity column Postgres will use generated value.
        See more: https://www.postgresqltutorial.com/postgresql-tutorial/postgresql-identity-column/
        */
        private static readonly IDictionary<string, string> _defaultPrimaryKeyGeneration = new Dictionary<string, string>() {
            { BIGIN, "GENERATED BY DEFAULT AS IDENTITY"},
            { BOOL, ""},
            { INT, "GENERATED BY DEFAULT AS IDENTITY" },
            { REAL, "" },
            { DOUBLE, "" },
            { DECIMAL, "" },
            { TEXT, ""},
            { TIMESTAMP, ""},
            { DATETIME, ""},
            { VA25, "" },
            { VA255,"" },
            { VA50, ""}
        };
        public static bool IsNumbericTypeCode(string typeCode) => _numbericTypeCodes.Contains(typeCode);
        public static string GetDataType(string typeCode) => _dataTypeMapping[typeCode];
        public static string GenerateAutoIncrement(string typeCode) => _defaultPrimaryKeyGeneration[typeCode];
        public static bool CheckDataType(string typeCode) => _dataTypeMapping.ContainsKey(typeCode);
        public static bool CheckDefaultValue(string typeCode, string defaultValue) =>
            !string.IsNullOrEmpty(defaultValue)
            ? _defaultValueMapping[typeCode].Invoke(defaultValue)
            : true;

        public static bool CheckDefaultValueNeedConvert(string typeCode) => _defaultValueTypeNeedConvert.Contains(typeCode);

        public static (bool Success, object Value) GetValue(object value, string typeCode, bool allowNull) => _valueParser[typeCode].Invoke(value, allowNull);
    }
}
