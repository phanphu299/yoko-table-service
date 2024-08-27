using System;

namespace AssetTable.Application.Constant
{
    public static class Privileges
    {
        public static string BASE_PATH = "tenants/{{tenantId}}/subscriptions/{{subscriptionId}}/applications/{{applicationId}}/projects/{{projectId}}/entities";
        public static string PRIVILEGES_PATH = "privileges";
        public static string PRIVILEGES_OBJECTS = "objects";
        public static string PRIVILEGES_NONE = "none";
        public static string PRIVILEGES_PROJECTS = "projects";

        public static string GetBasePath(Guid tenantId, Guid subscriptionId, string projectId)
        {
            return $"tenants/{tenantId}/subscriptions/{subscriptionId}/applications/{ApplicationInformation.APPLICATION_ID}/projects/{projectId}/entities";
        }
        public static string GetBasePathShort(Guid projectId)
        {
            return $"applications/{ApplicationInformation.APPLICATION_ID}/projects/{projectId}/entities";
        }

        public static class Asset
        {
            public const string ENTITY_NAME = "asset";
            public static class Rights
            {
                public const string WRITE_ASSET = "write_asset";
                public const string READ_ASSET = "read_asset";
                public const string READ_CHILD_ASSET = "read_child_asset";
                public const string DELETE_ASSET = "delete_asset";
                public const string ASSIGN_ASSET = "assign_asset";
            }
            public static class FullRights
            {
                public const string READ_ASSET = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset/read_asset";
                public const string WRITE_ASSET = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset/write_asset";
            }
            public static class Paths
            {
                public const string CHILDREN = "children";
            }
        }
        public static class AssetTable
        {
            public const string ENTITY_NAME = "asset_table";
            public static class Rights
            {
                public const string READ_ASSET_TABLE = "read_asset_table";
                public const string WRITE_ASSET_TABLE = "write_asset_table";
                public const string DELETE_ASSET_TABLE = "delete_asset_table";
            }
            public static class FullRights
            {
                public const string READ_ASSET_TABLE = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset_table/read_asset_table";
                public const string WRITE_ASSET_TABLE = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset_table/write_asset_table";
                public const string DELETE_ASSET_TABLE = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset_table/delete_asset_table";
            }
        }
        public static class Configuration
        {
            public const string ENTITY_NAME = "asset_table_configuration";
            public static class Rights
            {
                public const string SHARE_CONFIGURATION = "share_asset_table_configuration";
            }
            public static class FullRights
            {
                public const string SHARE_CONFIGURATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset_table_configuration/share_asset_table_configuration";
            }
        }
    }
}
