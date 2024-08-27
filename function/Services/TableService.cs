using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using System.Net.Http;
using AHI.Infrastructure.SharedKernel.Abstraction;
using System;

namespace AHI.AssetTable.Function.Service
{
    public class TableService : ITableService
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly ILoggerAdapter<TableService> _logger;
        private const int _maximumInsertItems = 500;

        public TableService(
            IConfiguration configuration,
            ITenantContext tenantContext,
            IHttpClientFactory httpClientFactory,
            ILoggerAdapter<TableService> logger)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _logger = logger;
        }


        public async Task UpdateAssetAsync(Guid assetId, string assetName, string resourcePath)
        {
            // var asset = await FetchAssetAsync(assetId);
            // if (asset == null)
            //     return;
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            using (var dbConnection = new NpgsqlConnection(connectionString))
            {
                var updateParams = new DynamicParameters();
                updateParams.Add("@AssetId", assetId);
                updateParams.Add("@AssetName", assetName);
                updateParams.Add("@ResourcePath", resourcePath);
                await dbConnection.ExecuteAsync(@"UPDATE tables
                                                  SET asset_name = @AssetName,
                                                  resource_path = @ResourcePath
                                                  WHERE asset_id = @AssetId;"
                                                  , updateParams);
                dbConnection.Close();
            }
        }
        public async Task DeleteTableAsync(Guid assetId, bool deleteTable)
        {
            if (deleteTable)
                await DeleteTableByAssetIdAsync(assetId);
            else
                await DetachTableByAssetIdAsync(assetId);
        }

        private async Task DeleteTableByAssetIdAsync(Guid assetId)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            using (var dbConnection = new NpgsqlConnection(connectionString))
            {
                var updateParams = new DynamicParameters();
                updateParams.Add("@AssetId", assetId);
                await dbConnection.ExecuteAsync(@"UPDATE tables
                                                    Set
                                                    deleted = true,
                                                    asset_id  = null,
                                                    asset_name = '',
                                                    resource_path = '',
                                                    updated_utc = current_timestamp 
                                                    where asset_id = @AssetId;"
                                                  , updateParams);
                dbConnection.Close();
            }
        }
        private async Task DetachTableByAssetIdAsync(Guid assetId)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            using (var dbConnection = new NpgsqlConnection(connectionString))
            {
                var updateParams = new DynamicParameters();
                updateParams.Add("@AssetId", assetId);
                await dbConnection.ExecuteAsync(@"UPDATE tables
                                                    set asset_id  = null,
                                                    asset_name = '',
                                                    resource_path = '',
                                                    updated_utc = current_timestamp 
                                                    where asset_id = @AssetId;"
                                                  , updateParams);
                dbConnection.Close();
            }
        }
    }
}
