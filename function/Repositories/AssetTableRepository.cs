using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AHI.AssetTable.Function.Constant;
using AHI.Infrastructure.Audit.Constant;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.Import.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Constant;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.AssetTable.Function.FileParser.Abstraction;
using AHI.Infrastructure.UserContext.Abstraction;
using Dapper;
using FluentValidation.Results;
using Function.Constant;
using Function.Model.ImportModel;
using Microsoft.Extensions.Configuration;
using Npgsql;
using AHI.Infrastructure.Service.Dapper.Extensions;

namespace Function.Repository
{
    public class AssetTableRepository : IEntityImportRepository<AssetTableModel>
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly IImportTrackingService _errorService;
        private readonly IParserContext _context;
        private readonly IUserContext _userContext;

        public AssetTableRepository(
            IConfiguration configuration,
            ITenantContext tenantContext,
            IImportTrackingService errorService,
            IParserContext context,
            IUserContext userContext
            )
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _errorService = errorService;
            _context = context;
            _userContext = userContext;
        }

        public async Task CommitAsync(Guid entityId, IEnumerable<AssetTableModel> source)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // get table name
                        // Validate asset table
                        await CheckExistsAssetTableAsync(entityId, connection);
                        var columns = await GetColumnAsync(entityId, connection);
                        ValidateRequiredColumn(columns, source);
                        // Fill default columns value regardless imported file have their data or not
                        FillDefaultValue(source);
                        var dbTableName = string.Format(DbName.Table.ASSET_TABLE, entityId);
                        var columnPrimary = columns.FirstOrDefault(x => x.IsPrimary);
                        if (PostgresDataTypeMapping.IDENTITY_TYPES.Contains(columnPrimary.DataType))
                        {
                            // seed to the max identity
                            //var seedMaxIdentityScript = $"SELECT setval(pg_get_serial_sequence('{dbTableName}', '{columnPrimary.Name}'), coalesce(MAX(\"{columnPrimary.Name}\"), 1)) from \"{dbTableName}\"";
                            var maxIdValueQuery = $"SELECT coalesce(MAX(\"{columnPrimary.Name}\"), 0) from \"{dbTableName}\"";
                            var maxId = await connection.ExecuteScalarAsync<int>(maxIdValueQuery, transaction: transaction);
                            maxId = maxId + 1;
                            var seedMaxIdentityScript = $"ALTER SEQUENCE \"{dbTableName}_{columnPrimary.Name}_seq\" RESTART WITH {maxId}";
                            await connection.ExecuteScalarAsync(seedMaxIdentityScript);

                        }
                        var offset = TimeSpan.Parse(_context.TimezoneOffset);
                        bool hasError = false;
                        foreach (var record in source)
                        {
                            try
                            {
                                // parse the appropriate type
                                var values = (from r in record
                                              join c in columns on r.Key.ToLowerInvariant() equals c.Name.ToLowerInvariant() // case insensitive
                                              select (c, r)).ToList();
                                var runValidationError = false;
                                var parameterKeys = values.Select(x => x.c.Name);
                                foreach (var value in values)
                                {
                                    var (columnInfo, importData) = value;
                                    var (isSuccess, errorCode) = ValidateInputValue(columnInfo, importData);
                                    if (!isSuccess)
                                    {
                                        // not good, the input data is invalid
                                        _errorService.RegisterError(errorCode, ErrorType.VALIDATING, new Dictionary<string, object>(record)
                                        {
                                            {"row", record.Row},
                                            {"propertyName", columnInfo.Name},
                                            {"propertyValue", importData.Value},
                                            {"propertyType", columnInfo.DataType}
                                        });
                                        hasError = true;
                                        runValidationError = true;
                                    }
                                    else
                                    {
                                        var importValue = importData.Value;
                                        if (importValue == null && !string.IsNullOrEmpty(columnInfo.DefaultValue))
                                        {
                                            // will using the default value
                                            importValue = columnInfo.DefaultValue;
                                            // for datetime, it's hard to parse the datetime
                                            if (columnInfo.DataType == PostgresDataTypeMapping.DATETIME)
                                            {
                                                var userDateTime = DateTime.ParseExact(columnInfo.DefaultValue, AHI.Infrastructure.SharedKernel.Extension.Constant.DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                                                // parse to user format
                                                importValue = userDateTime.ToString(_context.DateTimeFormat);
                                            }
                                        }

                                        if (importValue != null && columnInfo.DataType == PostgresDataTypeMapping.DATETIME && !columnInfo.IsSystemColumn)
                                        {
                                            var baseTime = DateTime.ParseExact(importValue.ToString(), _context.DateTimeFormat, CultureInfo.InvariantCulture);
                                            var sourceTime = new DateTimeOffset(baseTime, offset);
                                            var utcDate = sourceTime.UtcDateTime;
                                            record[columnInfo.Name] = utcDate;
                                        }
                                        else
                                        {
                                            record[columnInfo.Name] = importValue;
                                        }

                                        //Override system column
                                        var utcNow = DateTime.UtcNow;
                                        record[DefaultColumn.CREATED_BY] = _userContext.Upn;
                                        record[DefaultColumn.CREATED_UTC] = utcNow;
                                        record[DefaultColumn.UPDATED_UTC] = utcNow;
                                    }
                                }
                                // no need to continue insert into database if the data has error
                                if (runValidationError)
                                {
                                    continue;
                                }
                                object primaryValue = null;
                                if (record.ContainsKey(columnPrimary.Name))
                                {
                                    primaryValue = record[columnPrimary.Name];
                                }
                                if (primaryValue == null)
                                {
                                    // in case primary not specify, the primary should be auto generated
                                    //excludeKeys.Add(columnPrimary.Name);
                                    parameterKeys = parameterKeys.Except(new[] { columnPrimary.Name }).ToList();
                                }

                                //For conflic record
                                // "col1","col2","col3"
                                var columeNamesInRow = string.Join(',', parameterKeys.Select(x => x.ToStringQuote()));
                                // "col1"=EXCLUDED."col1","col2"=EXCLUDED."col2","col3"=EXCLUDED."col3"
                                var parameterKeyConflict = parameterKeys.Except(new[] { DefaultColumn.CREATED_BY, DefaultColumn.CREATED_UTC }).ToList();
                                var setStatementsInRow = string.Join(',', parameterKeyConflict.Select(x => $"{x.ToStringQuote()}=EXCLUDED.{x.ToStringQuote()}"));

                                // CAST(@col1 as X),CAST(@col2 as Y),CAST(@col3 as Z)
                                var paramsInRow = string.Join(',', parameterKeys.Select(x => ConvertToSqlParam(x, columns)));

                                var inserOrUpdateQuery = $"INSERT INTO {dbTableName.ToStringQuote()} ({columeNamesInRow}) VALUES ({paramsInRow}) ON CONFLICT ({columnPrimary.Name.ToStringQuote()}) DO UPDATE SET {setStatementsInRow};";
                                await connection.ExecuteScalarAsync(inserOrUpdateQuery, record, transaction, commandTimeout: 600);
                            }
                            catch (DbException e)
                            {
                                _errorService.RegisterError(e.Message, ErrorType.DATABASE);
                            }
                        }
                        if (!hasError)
                        {
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                        }
                    }
                    catch (Exception exc)
                    {
                        await transaction.RollbackAsync();
                        if (exc is EntityValidationException validationException)
                        {
                            var dictionary = new Dictionary<string, object>();
                            foreach (var f in validationException.Failures)
                            {
                                dictionary[f.Key] = f.Value;
                            }
                            _errorService.RegisterError(validationException.DetailCode, ErrorType.VALIDATING, dictionary);
                        }
                        else if (exc is EntityNotFoundException entityNotFoundException)
                        {
                            _errorService.RegisterError(entityNotFoundException.DetailCode, ErrorType.VALIDATING, entityNotFoundException.Payload);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
        }

        private (bool, string) ValidateInputValue(ColumnPostgres columnInfo, KeyValuePair<string, object> importData)
        {
            var value = importData.Value;
            // do not validate for primary and type in int, bigint
            if (columnInfo.IsPrimary && PostgresDataTypeMapping.IDENTITY_TYPES.Contains(columnInfo.DataType) && value is null)
            {
                return (true, null);
            }
            if (columnInfo.IsNull && value is null)
            {
                return (true, null);
            }
            // this value is null but column require not null
            if (columnInfo.IsNull == false && value is null)
            {
                // default value is not set
                if (string.IsNullOrEmpty(columnInfo.DefaultValue))
                {
                    return (false, ImportErrorMessage.IMPORT_ERROR_REQUIRED);
                }
                else
                {
                    return (true, null);
                }
            }
            try
            {
                switch (columnInfo.DataType)
                {
                    case PostgresDataTypeMapping.DATETIME:
                        value.ValidateDateTime(_context.DateTimeFormat);
                        break;
                    case PostgresDataTypeMapping.BOOL:
                        value.ValidateBoolean();
                        break;
                    case PostgresDataTypeMapping.INT:
                        var v = value.ValidateInteger();
                        break;
                    case PostgresDataTypeMapping.BIGIN:
                        value.ValidateLong();
                        break;
                    case PostgresDataTypeMapping.DOUBLE:
                        value.ValidateDouble();
                        break;
                    case PostgresDataTypeMapping.REAL:
                        value.ValidateFloat();
                        break;
                    case PostgresDataTypeMapping.TIMESTAMP:
                        value.ValidateTimestamp();
                        break;
                    case PostgresDataTypeMapping.VA25:
                        value.ValidateText(25);
                        break;
                    case PostgresDataTypeMapping.VA50:
                        value.ValidateText(50);
                        break;
                    case PostgresDataTypeMapping.VA255:
                        value.ValidateText(255);
                        break;
                }
            }
            catch (FormatException)
            {
                // invalid format
                return (false, ImportErrorMessage.IMPORT_ERROR_GENERAL_INVALID);
            }
            catch (OverflowException)
            {
                // valid format but the value is too small or too big
                return (false, ImportErrorMessage.IMPORT_ERROR_GENERAL_OUT_OF_RANGE);
            }
            return (true, null);
        }

        private static void ValidateRequiredColumn(IEnumerable<ColumnPostgres> columns, IEnumerable<AssetTableModel> sources)
        {
            var validationItem = sources.First();
            string[] excludeKeys = Array.Empty<string>();
            var primaryColumn = columns.First(x => x.IsPrimary);
            if (PostgresDataTypeMapping.IDENTITY_TYPES.Contains(primaryColumn.DataType))
            {
                excludeKeys = new[] { primaryColumn.Name };
            }
            var headers = validationItem.Keys;
            var missingColumns = (from c in columns.Where(x => !x.IsSystemColumn)
                                  join r in headers on c.Name.ToLowerInvariant() equals r.ToLowerInvariant() into gj // case insensitive
                                  from j in gj.DefaultIfEmpty()
                                  where c.IsNull == false && !excludeKeys.Contains(c.Name) && j == default
                                  select c).ToList();
            if (missingColumns.Any())
            {
                // not good, missing required columns
                var failures = missingColumns.Select(column => new ValidationFailure(column.Name, $"Column: {column.Name} is missing")).ToList();
                throw new EntityValidationException(failures, detailCode: ImportErrorMessage.IMPORT_ERROR_INVALID_TABLE_SCHEMA);
            }
        }

        private static string ConvertToSqlParam(string columnName, IEnumerable<ColumnPostgres> columns)
        {
            var column = columns.FirstOrDefault(x => x.Name == columnName);

            if (column == null)
            {
                throw new InvalidOperationException($"Column {columnName} doesn't exist.");
            }

            var dataType = PostgresDataTypeMapping.DataTypeMapping[column.DataType];

            return $"CAST(@{columnName} as {dataType})";
        }

        private static async Task<IEnumerable<ColumnPostgres>> GetColumnAsync(Guid tableId, NpgsqlConnection connection)
        {
            var query = $"select name as Name, type_code as Datatype, is_primary as IsPrimary, allow_null as IsNull, default_value as DefaultValue, is_system_column as IsSystemColumn from columns where table_id = @TableId";
            return await connection.QueryAsync<ColumnPostgres>(query, new { TableId = tableId });
        }

        private static async Task CheckExistsAssetTableAsync(Guid tableId, NpgsqlConnection connection)
        {
            var query = $"SELECT EXISTS (SELECT FROM tables WHERE id = @TableId)";
            var exists = await connection.QueryFirstOrDefaultAsync<bool>(query, new { TableId = tableId });
            if (exists == false)
            {
                throw new EntityNotFoundException();
            }
        }

        private void FillDefaultValue(IEnumerable<AssetTableModel> source)
        {
            foreach (var record in source)
            {
                record[DefaultColumn.CREATED_BY] = _userContext.Upn;
                record[DefaultColumn.CREATED_UTC] = DateTime.UtcNow.ToString(_context.DateTimeFormat);
                record[DefaultColumn.UPDATED_UTC] = DateTime.UtcNow.ToString(_context.DateTimeFormat);
            }
        }

        internal class ColumnPostgres
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsPrimary { get; set; }
            public bool IsNull { get; set; }
            public string DefaultValue { get; set; }
            public bool IsSystemColumn { get; set; }
        }
    }
}