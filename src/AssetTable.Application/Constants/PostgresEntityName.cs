using System.Linq;
using System.Text.RegularExpressions;

namespace AssetTable.Application.Constant
{
    public static class PostgresEntityName
    {
        private static readonly string[] _invalidNames =
        {
            " ",
            "varchar(25)",
            "varchar(255)",
            "varchar(50)"
        };
        //cause table/column like Int, real cant make in psql
        public static bool CheckValidName(string name) => !_invalidNames.Contains(name.ToLower()) && !(new Regex("[^a-zA-Z0-9_]").IsMatch(name.ToLower())) && !name.Contains(" ");

    }
}
