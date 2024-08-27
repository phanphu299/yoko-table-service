namespace AssetTable.Application.Constant
{
    public static class CacheKey
    {
        /// <summary>
        /// {0}: projectId
        /// Used in device service, function block service, asset media service, asset table service
        /// </summary>
        public const string ASSET_HASH_KEY = "assets:{0}"; // projectId
        public const string ASSET_HASH_FIELD = "{0}"; // assetId

        // <summary>
        /// {0}: projectId
        /// Used in asset table service
        /// </summary>
        public const string TABLE_HASH_KEY = "tables:{0}"; // projectId
        public const string TABLE_HASH_FIELD = "{0}"; // tableId
    }
}