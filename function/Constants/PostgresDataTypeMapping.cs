using System.Collections.Generic;

namespace Function.Constant
{
    public static class PostgresDataTypeMapping
    {
        public const string BIGIN = "BIGIN";
        public const string BOOL = "BOOL";
        public const string INT = "INT";
        public const string REAL = "REAL";
        public const string DOUBLE = "DOUBLE";
        public const string TEXT = "TEXT";
        public const string TIMESTAMP = "TIMESTAMP";
        public const string DATETIME = "DATETIME";
        public const string VA25 = "VA25";
        public const string VA255 = "VA255";
        public const string VA50 = "VA50";
        public static readonly string[] IDENTITY_TYPES = new[]{
            INT, BIGIN
        };
        public static readonly IDictionary<string, string> DataTypeMapping = new Dictionary<string, string>()
        {
            { BIGIN, "bigint" },
            { BOOL, "boolean" },
            { INT, "int" },
            { REAL, "real" },
            { DOUBLE,"double precision"},
            { TEXT, "text" },
            { TIMESTAMP, "bigint" },
            { DATETIME, "timestamp without time zone"},
            { VA25, "varchar(25)" },
            { VA255, "varchar(255)" },
            { VA50, "varchar(50)" }
        };
    }
}
