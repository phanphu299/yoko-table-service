namespace AssetTable.Application.Constant
{
    public static class DataTypeConstants
    {
        public const string TYPE_TEXT = "text";
        public const string TYPE_BOOLEAN = "bool";
        public const string TYPE_TIMESTAMP = "timestamp";
        public const string TYPE_DOUBLE = "double";
        public const string TYPE_INTEGER = "int";
        public const string TYPE_DATETIME = "datetime";
        public static string[] NUMBERIC_TYPES = new string[] { TYPE_BOOLEAN, TYPE_DOUBLE, TYPE_INTEGER };
        public static string[] TEXT_TYPES = new string[] { TYPE_TEXT };
    }
}
