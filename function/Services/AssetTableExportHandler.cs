using System.Threading.Tasks;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using AHI.Infrastructure.SharedKernel.Extension;
using Npgsql;
using Dapper;
using System.Linq;
using AHI.AssetTable.Function.Constant;
using Function.FileParser;
using System.Data;
using AHI.Infrastructure.Exception;
using AHI.AssetTable.Function.Service.Abstraction;
using AHI.Infrastructure.Import.Abstraction;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AHI.Infrastructure.SharedKernel;
using AHI.Infrastructure.Service.Dapper.Abstraction;
using Function.Extension;
using AHI.Infrastructure.Service.Dapper.Model;
using AHI.Infrastructure.Service.Dapper.Extensions;

namespace AHI.AssetTable.Function.Service
{
    public class AssetTableExportHandler : IAssetTableExportHandler
    {
        private readonly IConfiguration _configuration;
        private const string DEFAULT_SHEET_NAME = "Sheet 1";

        public AssetTableExportHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly ITenantContext _tenantContext;
        private readonly IStorageService _storageService;
        private readonly IQueryService _queryService;
        private readonly IParserContext _context;

        public AssetTableExportHandler(
            IConfiguration configuration,
            ITenantContext tenantContext,
            IStorageService storageService,
            IQueryService queryService,
            IParserContext context)
        {
            _tenantContext = tenantContext;
            _storageService = storageService;
            _queryService = queryService;
            _configuration = configuration;
            _context = context;
        }

        public async Task<string> HandleAsync(string workingDirectory, Guid tableId, string filter)
        {
            var path = Path.Combine(workingDirectory, "AppData", "ExportTemplate", "AssetTableModel.xlsx");

            return await ProcessExportAssetTableAsync(tableId, filter, path);
        }

        private async Task<string> ProcessExportAssetTableAsync(Guid tableId, string filter, string path)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var excelExportBuilder = new AssetTableExcelExport(_context);
            var fileName = string.Empty;
            using (var dbConnection = new NpgsqlConnection(connectionString))
            {
                excelExportBuilder.SetTemplate(path);

                var query = $"SELECT id as Id, name as Name FROM tables WHERE id = @TableId;";
                var table = await dbConnection.QueryFirstOrDefaultAsync<AssetTable>(query, new { TableId = tableId });

                if (table == null)
                    throw new EntityNotFoundException();

                fileName = $"{table.Name}_{DateTime.UtcNow.ToTimestamp(_context.TimezoneOffset)}.xlsx";
                var queryGetColumn = $@"SELECT name AS Name, type_code AS Datatype, is_primary AS IsPrimary, is_system_column AS IsSystemColumn 
                                        FROM columns WHERE table_id = @TableId 
                                        ORDER BY is_system_column, is_primary DESC, id;";
                var columns = await dbConnection.QueryAsync<ColumnPostgres>(queryGetColumn, new { TableId = table.Id });
                var primaryColumn = columns.First(x => x.IsPrimary);
                var columnNames = string.Join(',', columns.Select(x => x.Name.ToStringQuote()));
                var dbTableName = string.Format(DbName.Table.ASSET_TABLE, table.Id);
                if (string.IsNullOrWhiteSpace(filter))
                {
                    var queryGetData = $"SELECT {columnNames} FROM {dbTableName.ToStringQuote()} ORDER BY \"{primaryColumn.Name}\" ASC;";
                    var data = await dbConnection.QueryAsync(queryGetData);
                    excelExportBuilder.SetData(DEFAULT_SHEET_NAME, table.Name, tableId, columns, data);
                }
                else
                {
                    var pagingSqlScript = $"SELECT {columnNames} FROM \"{dbTableName}\"";
                    var queryCriteria = new QueryCriteria
                    {
                        Filter = JsonConvert.DeserializeObject(filter, Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting) as JObject,
                    };

                    ExpandoObject pagingValue;
                    (pagingSqlScript, pagingValue) = _queryService.CompileQuery(pagingSqlScript, queryCriteria, paging: false);

                    var data = await dbConnection.QueryAsync(pagingSqlScript, pagingValue);
                    excelExportBuilder.SetData(DEFAULT_SHEET_NAME, table.Name, tableId, columns, data);
                }
                await dbConnection.CloseAsync();
            }
            return await _storageService.UploadAsync("sta/files/temp/exports", fileName, excelExportBuilder.BuildExcelStream());
        }

        internal class AssetTable
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class ColumnPostgres
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsPrimary { get; set; }
        }
    }
}
