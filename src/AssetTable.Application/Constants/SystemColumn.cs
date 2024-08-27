namespace AssetTable.Application.Constant
{
    public static class SystemColumn
    {
        public const string CREATED_BY = "created_by";
        public const string CREATED_UTC = "created_utc";
        public const string UPDATED_UTC = "updated_utc";
        public static readonly string[] ALL_COLUMNS = new string[] { CREATED_BY, CREATED_UTC, UPDATED_UTC };
    }
}
