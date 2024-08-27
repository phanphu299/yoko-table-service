namespace AHI.AssetTable.Function.Constant
{
    public static class ActivityLogEntityNames
    {
        public const string ASSET_TABLE_ENTITY = "Asset Table";

        public static string GetActivityLogEntityName(string objectType)
        {
            return objectType switch
            {
                IOEntityType.ASSET_TABLE => ActivityLogEntityNames.ASSET_TABLE_ENTITY,
                _ => null
            };
        }
    }
}
