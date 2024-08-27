using System.Collections.Generic;

namespace AssetTable.Application.Constant
{
    public static class PostgresColumnTypeMapping
    {
        private static readonly IDictionary<bool, string> _columnTypeMapping = new Dictionary<bool, string>() {
            { false, "not null" },
            { true, "null" }
        };

        private static readonly IDictionary<bool, string> _columnStateMapping = new Dictionary<bool, string>() {
            { false, "set not null" },
            { true, "drop not null" }
        };

        public static string GetColumnType(bool allowNull) => _columnTypeMapping[allowNull];
        public static string GetColumnState(bool allowNull) => _columnStateMapping[allowNull];
    }
}
