using System;
using System.Data;

namespace AssetTable.ApplicationExtension.Extension
{
    public static class DbTypeExtensions
    {
        public static object GetDefaultValue(this DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return "";
                case DbType.Binary:
                    return new byte[0];
                case DbType.Boolean:
                    return false;
                case DbType.Byte:
                    return default(byte);
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                case DbType.VarNumeric:
                    return 0m;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return default(DateTime);
                case DbType.Guid:
                    return default(Guid);
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return 0;
                case DbType.SByte:
                    return default(sbyte);
                case DbType.Time:
                    return TimeSpan.Zero;
                case DbType.Object:
                    return null;
                default:
                    throw new ArgumentException($"Unsupported DbType: {dbType}");
            }
        }
    }
}