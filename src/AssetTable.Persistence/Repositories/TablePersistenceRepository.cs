using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Dynamic;
using AHI.Infrastructure.Exception;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.Repository.Generic;
using AHI.Infrastructure.SharedKernel.Extension;
using AssetTable.Application.Repository;
using AssetTable.Domain.Entity;
using AssetTable.Persistence.Context;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Constant;
using AssetTable.ApplicationExtension.Extension;
using Npgsql;
using AssetTable.Application.Service;
using AssetTable.Application.Extension;
using System.Globalization;
using Newtonsoft.Json;
using AHI.Infrastructure.SharedKernel.Abstraction;

namespace AssetTable.Persistence.Repository
{
    public class TablePersistenceRepository : GenericRepository<Table, Guid>, ITableRepository
    {
        private readonly TableDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly ILoggerAdapter<TablePersistenceRepository> _logger;
        private const string DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss:ffff";

        public TablePersistenceRepository(TableDbContext context, IConfiguration configuration, ITenantContext tenantContext, ILoggerAdapter<TablePersistenceRepository> logger)
            : base(context)
        {
            _dbContext = context;
            _logger = logger;
            _configuration = configuration;
            _tenantContext = tenantContext;
        }

        protected override void Update(Table requestObject, Table targetObject)
        {
            targetObject.OldName = targetObject.Name;
            targetObject.Name = requestObject.Name;
            targetObject.Description = requestObject.Description;
            targetObject.UpdatedUtc = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(requestObject.Script))
                targetObject.Script = requestObject.Script;
        }

        public override IQueryable<Table> AsQueryable()
        {
            return _dbContext.Tables.Include(x => x.Columns)
                                .Include(x => x.EntityTags)
                                .Where(x => !x.EntityTags.Any() || x.EntityTags.Any(a => a.EntityType == EntityTypeConstants.TABLE));
        }

        public Task<bool> CheckExistNameAsync(string name, Guid assetId, Guid? id = null)
        {
            return id == null ?
                _dbContext.Tables.AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.AssetId == assetId) :
                _dbContext.Tables.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower() && x.AssetId == assetId);
        }

        public async Task<Table> FindTableAsync(Guid id)
        {
            var table = await _dbContext.Tables
                                            .Include(x => x.Columns)
                                            .Include(x => x.EntityTags)
                                            .Where(x => !x.EntityTags.Any() || x.EntityTags.Any(a => a.EntityType == EntityTypeConstants.TABLE))
                                            .FirstOrDefaultAsync(x => x.Id == id);
            return table;
        }

        public async Task<AssetTableDto> GetAssetTableByIdAsync(Guid id)
        {
            try
            {
                using (var connection = GetReadOnlyDbConnection())
                {
                    var query = @"select
                                    at.id as TableId,
                                    at.name as TableName,
                                    at.description as TableDescription,
                                    at.asset_id as AssetId,
                                    at.resource_path as ResourcePath,
                                    at.created_by as CreatedBy,
                                    atc.id as ColumnId,
                                    atc.name as ColumnName,
                                    atc.is_primary as ColumnIsPrimary,
                                    atc.type_code as ColumnTypeCode,
                                    atc.default_value as ColumnDefaultValue,
                                    atc.allow_null as ColumnAllowNull,
                                    atc.is_system_column as IsSystemColumn
                                from tables at
                                    join columns atc on atc.table_id = at.id
                                where at.id = @Id and at.deleted = false and atc.deleted = false";

                    var assetTableDictionary = new Dictionary<Guid, AssetTableDto>();
                    var assetColumns = new List<AssetColumnDto>();
                    var assetTables = await connection.QueryAsync<AssetTableDto, AssetColumnDto, AssetTableDto>(
                        query,
                        (assetTable, assetColumn) =>
                        {
                            AssetTableDto assetTableEntry;
                            if (!assetTableDictionary.TryGetValue(assetTable.AssetId, out assetTableEntry))
                            {
                                assetTableEntry = assetTable;
                                assetTableDictionary.Add(assetTable.AssetId, assetTableEntry);
                            }
                            assetColumns.Add(assetColumn);
                            return assetTableEntry;
                        },
                        splitOn: "ColumnId",
                        param: new { Id = id }
                    );

                    connection.Close();

                    var assetTable = assetTableDictionary.Values.FirstOrDefault();
                    if (assetTable != null)
                    {
                        assetTable.Columns = assetColumns;
                    }

                    return assetTable;
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                throw new GenericProcessFailedException(detailCode: MessageConstants.DATABASE_QUERY_FAILED, innerException: ex);
            }
        }

        private async Task LoadParentElementAsync(Asset entity)
        {
            await _dbContext.Entry(entity).Navigation("ParentAsset").LoadAsync();
            if (entity.ParentAsset != null)
            {
                await LoadParentElementAsync(entity.ParentAsset);
            }
        }

        public async Task<Table> AddTableAsync(Table entity)
        {
            await _dbContext.Tables.AddAsync(entity);
            await _dbContext.Database.ExecuteSqlRawAsync(entity.Script);
            return entity;
        }

        public async Task<Table> UpdateTableAsync(Table requestEntity, Table targetEntity)
        {
            Update(requestEntity, targetEntity);
            if (!string.IsNullOrEmpty(requestEntity.Script))
            {
                targetEntity.Columns = requestEntity.Columns;
                await _dbContext.Database.ExecuteSqlRawAsync(targetEntity.Script);
            }
            return targetEntity;
        }

        public async Task<bool> DeleteTablesAsync(IEnumerable<Table> tables)
        {
            foreach (var table in tables)
            {
                var tableName = TableService.GetTableName(table.Id);
                await _dbContext.Database.ExecuteSqlRawAsync($"drop table if exists {tableName.ToStringQuote()};");
            }

            foreach (var table in tables)
            {
                table.AssetId = null;
                table.Deleted = true;
                table.UpdatedUtc = DateTime.UtcNow;
            }

            return true;
        }

        public async Task<IEnumerable<object>> GetTableDataAsync(string query, ExpandoObject value = null)
        {
            using (var connection = GetReadOnlyDbConnection())
            {
                var result = await connection.QueryAsync<object>(query, value);
                connection.Close();
                var data = result.Select(x => (ExpandoObject)x.ToExpandoObject());
                return data;
            }
        }

        public async Task<int> CountTableDataAsync(string query, ExpandoObject value = null)
        {
            using (var connection = GetReadOnlyDbConnection())
            {
                var result = await connection.QueryFirstAsync<int>(query, value);
                connection.Close();
                return result;
            }
        }

        public async Task<object> GetTableAggregationDataAsync(string aggregationQuery, object value)
        {
            using (var connection = GetReadOnlyDbConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<double>(aggregationQuery, value);
                connection.Close();
                return result;
            }
        }

        public async Task UpsertTableDataAsync(string upsertQuery, ExpandoObject value = null)
        {
            try
            {
                using (var connection = GetDbConnection())
                {
                    var result = await connection.ExecuteAsync(upsertQuery, value);
                    connection.Close();
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                var errorLog = new
                {
                    Query = upsertQuery,
                    Value = value
                };
                _logger.LogError(ex, $"UpsertTableDataAsync failed! {JsonConvert.SerializeObject(errorLog)}");
                if (ex.SqlState == PostgreSQLState.UNIQUE_VIOLATION)
                {
                    throw new GenericProcessFailedException(detailCode: MessageConstants.DATABASE_UNIQUE_VIOLATION, innerException: ex);
                }
                throw new GenericProcessFailedException(detailCode: MessageConstants.DATABASE_QUERY_FAILED, innerException: ex);
            }
        }

        public async Task<bool> CheckHasDataTableAsync(Guid tableId)
        {
            using (var connection = GetReadOnlyDbConnection())
            {
                var tableName = TableService.GetTableName(tableId);
                var data = await connection.QueryAsync<object>($"SELECT 1 FROM {tableName.ToStringQuote()} limit 1");
                connection.Close();
                return data.Any();
            }
        }

        public async Task<bool> CheckColumnHasNullDataAsync(Guid tableId, IEnumerable<string> columns)
        {
            using (var connection = GetReadOnlyDbConnection())
            {
                var tableName = TableService.GetTableName(tableId);
                var query = string.Empty;
                foreach (var column in columns)
                {
                    if (string.IsNullOrEmpty(query))
                        query += $"{column.ToStringQuote()} is null ";
                    else
                        query += $"or {column.ToStringQuote()} is null ";
                }
                var checkQuery = $"SELECT exists (SELECT 1 FROM {tableName.ToStringQuote()} WHERE {query} LIMIT 1);";
                var existsNullData = await connection.QueryFirstOrDefaultAsync<bool>(checkQuery);
                connection.Close();
                return existsNullData;
            }
        }

        public virtual Task<bool> RemoveListEntityWithRelationAsync(ICollection<Table> assetTables)
        {
            foreach (Table item in assetTables)
            {
                item.Deleted = true;
            }
            return Task.FromResult(true);
        }

        public async Task<IEnumerable<ArchiveAssetDto>> ArchiveAsync(DateTime ArchiveTime)
        {
            using (var connection = GetReadOnlyDbConnection())
            {
                var assetTables = new List<ArchiveAssetDto>();

                var query = $"select table_name as TableName from information_schema.tables where table_schema = 'public' and table_name like 'asset_%'";

                var tables = await connection.QueryAsync<ArchiveAssetDto>(query, commandTimeout: 600);

                foreach (var table in tables)
                {
                    var queryGetData = $@"SELECT * FROM {table.TableName.ToStringQuote()}";
                    var tableData = await connection.QueryAsync<object>(queryGetData, commandTimeout: 600);
                    table.Data = tableData;
                    assetTables.Add(table);
                }
                connection.Close();
                return assetTables;
            }
        }

        public Task RetrieveAsync(IEnumerable<Table> tables)
        {
            _dbContext.Database.SetCommandTimeout(RetrieveConstants.TIME_OUT);
            return _dbContext.Tables.AddRangeAsync(tables);
        }

        public async Task GenerateTablesAsync(IEnumerable<Table> tables, IEnumerable<ArchiveAssetDto> assetTables)
        {
            using (var connection = GetDbConnection())
            {
                foreach (var item in tables)
                {
                    var tableName = string.Format(TableName.PATTERN, item.Id);
                    // Clean up un-correct data
                    var primaryColumns = item.Columns.Where(x => x.IsPrimary).ToList();
                    if (primaryColumns.Count > 1)
                    {
                        //Should always keep the first one and remove others
                        for (int i = 1; i < primaryColumns.Count; i++)
                        {
                            item.Columns.Remove(primaryColumns[i]);
                        }
                    }

                    // Execute the query create asset tables
                    var tableScriptBuilder = new AddTableScriptBuilder(TableDto.Create(item));
                    var scriptCreateTable = tableScriptBuilder.Validate().BuildTable().GetScript();
                    await connection.ExecuteAsync(scriptCreateTable);

                    // Build the SQL query and parameter list dynamically
                    string insertQuery = $@"INSERT INTO {tableName.ToStringQuote()} ({string.Join(", ", item.Columns.Select(x => x.Name.ToStringQuote()))}) VALUES ({string.Join(", ", item.Columns.Select(c => "@" + c.Name))})";
                    var parametersList = new List<DynamicParameters>();
                    var assetTable = assetTables.FirstOrDefault(x => x.TableName == tableName);
                    if (assetTable == null || assetTable.Data == null)
                        throw new EntityInvalidException();

                    bool validRecord = true;
                    foreach (var record in assetTable.Data)
                    {
                        var parameters = new DynamicParameters();
                        foreach ((string columnName, string typeCode, bool allowNull) in item.Columns.Select(x => (x.Name, x.TypeCode, x.AllowNull)))
                        {
                            DbType dbType = typeCode.GetDataType().ToDbType();
                            if (record.ContainsKey(columnName.ToCamelCase()))
                            {
                                var value = record[columnName.ToCamelCase()].ToObject<object>();
                                if (value != null && dbType == DbType.DateTime)
                                {
                                    value = DateTime.ParseExact(value, DATE_TIME_FORMAT, CultureInfo.CurrentCulture);
                                }
                                if (value == null && !allowNull)
                                {
                                    value = dbType.GetDefaultValue();
                                }
                                parameters.Add("@" + columnName, value, dbType);
                            }
                            else // column not exist
                            {
                                validRecord = false;
                            }
                        }

                        if (validRecord)
                        {
                            parametersList.Add(parameters);
                        }
                        else
                        {
                            _logger.LogError($"Record {JsonConvert.SerializeObject(record)} having column not exist in Table {tableName}");
                        }
                    }

                    // Execute the SQL query
                    await connection.ExecuteAsync(insertQuery, parametersList);
                }
                var columns = tables.SelectMany(x => x.Columns);
                var maxId = columns.Any() ? columns.Max(x => x.Id) : 0;
                var updateSeq = $"select setval('columns_id_seq', {maxId + 1}, false);";
                await connection.ExecuteAsync(updateSeq);
                connection.Close();
            }
        }

        protected IDbConnection GetDbConnection()
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }

        protected IDbConnection GetReadOnlyDbConnection()
        {
            var connectionString = _configuration["ConnectionStrings:ReadOnly"] ?? _configuration["ConnectionStrings:Default"];
            connectionString = connectionString.BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }
    }
}
