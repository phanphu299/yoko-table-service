using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Audit.Constant;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.Exception.Helper;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.Service;
using AHI.Infrastructure.Service.Dapper.Abstraction;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.UserContext.Abstraction;
using AHI.Infrastructure.UserContext.Service.Abstraction;
using AssetTable.Application.AssetTable.Command;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Constant;
using AssetTable.Application.Events;
using AssetTable.Application.Extension;
using AssetTable.Application.FileRequest.Command;
using AssetTable.Application.Models;
using AssetTable.Application.Repository;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.TableList.Command;
using AssetTable.Application.TableList.Command.Model;
using AssetTable.ApplicationExtension.Extension;
using AssetTable.Domain.Entity;
using Device.Application.Constant;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AssetTable.Application.Service
{
    public class TableService : BaseSearchService<Table, Guid, GetTableListByCriteria, GetTableListDto>, ITableService
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;
        private readonly IEntityLockService _entityLockService;
        private readonly ITableUnitOfWork _unitOfWork;
        private readonly IQueryService _queryService;
        private readonly IFileEventService _fileEventService;
        private readonly IValidator<ArchiveAssetTableDto> _validator;
        private readonly ILoggerAdapter<TableService> _logger;
        private readonly ITenantContext _tenantContext;
        private readonly ICache _cache;
        private readonly ITagService _tagService;

        public TableService(
            IServiceProvider serviceProvider,
            IAuditLogService auditLogService,
            IUserContext userContext,
            ISecurityService securityService,
            ITableUnitOfWork unitOfWork,
            IQueryService queryService,
            IFileEventService fileEventService,
            IEntityLockService entityLockService,
            ILoggerAdapter<TableService> logger,
            IValidator<ArchiveAssetTableDto> validator,
            ITenantContext tenantContext,
            ITagService tagService,
            ICache cache)
            : base(GetTableListDto.Create, serviceProvider)
        {
            _auditLogService = auditLogService;
            _userContext = userContext;
            _securityService = securityService;
            _unitOfWork = unitOfWork;
            _queryService = queryService;
            _fileEventService = fileEventService;
            _entityLockService = entityLockService;
            _validator = validator;
            _logger = logger;
            _tenantContext = tenantContext;
            _cache = cache;
            _tagService = tagService;
        }

        public async Task<GetTableListDto> FindAssetTableByIdAsync(GetTableListById command, CancellationToken token)
        {
            var element = await _unitOfWork.Table.FindAsync(command.Id);
            if (element == null)
                throw new EntityNotFoundException();

            return GetTableListDto.Create(element);
        }

        public async Task<BaseResponse> RemoveAssetTablesAsync(DeleteTableList command, CancellationToken token)
        {
            if (command?.Ids == null || command.Ids.Length < 1)
                return BaseResponse.Success;
            var ids = command.Ids.Distinct().ToArray();
            var deleteTableNames = new List<string>();
            bool result = true;
            try
            {
                //check deleting id in asset table like {id::number}
                var assetTables = await _unitOfWork.Table.AsQueryable().Where(x => ids.Contains(x.Id)).ToArrayAsync(cancellationToken: token);
                if (ids.Length != assetTables.Length)
                {
                    throw new EntityNotFoundException(detailCode: ExceptionErrorCode.DetailCode.ERROR_ENTITY_NOT_FOUND_SOME_ITEMS_DELETED);
                }

                deleteTableNames.AddRange(assetTables.Select(x => x.Name));

                foreach (var table in assetTables)
                {
                    //Check permision
                    _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, table.ResourcePath, ownerUpn: table.CreatedBy, throwException: true);
                    _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.WRITE_ASSET_TABLE, table.ResourcePath, ownerUpn: table.CreatedBy, throwException: true);
                }

                result = await _unitOfWork.Table.RemoveListEntityWithRelationAsync(assetTables);
                await _unitOfWork.EntityTag.RemoveByEntityIdsAsync(EntityTypeConstants.TABLE, ids, isTracking: true);

                await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Delete, ActionStatus.Success, command.Ids, deleteTableNames, command);
                await _unitOfWork.CommitAsync();
            }
            catch (System.Exception ex)
            {
                await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Delete, ex, command.Ids, deleteTableNames, command);
                throw;
            }

            var hashKey = CacheKey.TABLE_HASH_KEY.GetCacheKey(_tenantContext.ProjectId);
            var deleteFields = ids.Select(x =>
                            {
                                return CacheKey.TABLE_HASH_FIELD.GetCacheKey(x);
                            }).ToList();

            await _cache.DeleteHashByKeysAsync(hashKey, deleteFields);

            return new BaseResponse(result, string.Empty);
        }

        public Task<(string ResourcePath, string AssetCreatedBy)> GetResourcePathByAssetIdAsync(Guid assetId, CancellationToken token)
        {
            return _unitOfWork.Table.AsQueryable().Where(x => x.AssetId == assetId).Select(x => ValueTuple.Create(x.ResourcePath, x.AssetCreatedBy)).FirstOrDefaultAsync();
        }

        protected override Type GetDbType() { return typeof(ITableRepository); }

        public async Task<BaseResponse> ValidateExistAssetTablesAsync(CheckExistingTableList command, CancellationToken token)
        {
            var requestIds = new HashSet<Guid>(command.Ids.Distinct());
            var existingIds = await _unitOfWork.Table
                .AsQueryable()
                .AsNoTracking()
                .Where(x => requestIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var tables = new HashSet<Guid>(existingIds);

            if (!requestIds.SetEquals(tables))
                throw new EntityNotFoundException();

            return BaseResponse.Success;
        }

        public async Task<AddTableDto> AddTableAsync(AddTable command, CancellationToken cancellationToken)
        {
            var entity = AddTable.Create(command);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (await _unitOfWork.Table.CheckExistNameAsync(entity.Name, command.AssetId))
                {
                    throw ValidationExceptionHelper.GenerateDuplicateValidation(nameof(Domain.Entity.Table.Name));
                }

                var tableScriptBuilder = new AddTableScriptBuilder(TableDto.Create(entity));
                var scriptCreateTable = tableScriptBuilder.Validate().BuildTable().GetScript();

                entity.Script = scriptCreateTable;
                entity.AssetName = command.AssetName;
                entity.ResourcePath = command.ResourcePath;
                entity.CreatedBy = command.CreatedBy;
                entity.AssetCreatedBy = command.AssetCreatedBy;

                var tagIds = Array.Empty<long>();
                command.Upn = _userContext.Upn;
                command.ApplicationId = Guid.Parse(!string.IsNullOrEmpty(_userContext.ApplicationId) ? _userContext.ApplicationId : ApplicationInformation.APPLICATION_ID);
                if (command.Tags != null && command.Tags.Any())
                {
                    tagIds = await _tagService.UpsertTagsAsync(command);
                }

                if (tagIds.Any())
                {
                    var entitiesTags = tagIds.Distinct().Select(x => new EntityTagDb
                    {
                        EntityType = EntityTypeConstants.TABLE,
                        EntityIdGuid = entity.Id,
                        TagId = x
                    }).ToArray();
                    entity.EntityTags = entitiesTags;
                }

                await _unitOfWork.Table.AddTableAsync(entity);
                await _unitOfWork.CommitAsync();
            }
            catch (System.Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Add, ex, payload: command);
                throw;
            }
            await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Add, ActionStatus.Success, entity.Id, entity.Name, command);

            return await _tagService.FetchTagsAsync(AddTableDto.Create(entity));
        }

        public async Task<UpdateTableDto> UpdateTableAsync(UpdateTable command, CancellationToken cancellationToken)
        {
            Table targetEntity = null;
            var entityLock = await _entityLockService.GetLockEntityAsync(command.Id, cancellationToken);
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                targetEntity = await _unitOfWork.Table.FindTableAsync(command.Id);
                if (targetEntity == null)
                {
                    throw new EntityNotFoundException();
                }

                if (command.Columns.Any(c => c.Action != ColumnAction.NO) &&
                    entityLock != null &&
                    entityLock.CurrentUserUpn != _userContext.Upn)
                {
                    throw new EntityAlreadyLockException();
                }

                _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, targetEntity.ResourcePath, targetEntity.CreatedBy, throwException: true);
                _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.READ_ASSET_TABLE, targetEntity.ResourcePath, targetEntity.CreatedBy, throwException: true);

                if (targetEntity.AssetId == null || targetEntity.AssetId == Guid.Empty)
                {
                    throw new EntityNotFoundException(detailCode: MessageConstants.ASSET_NOT_FOUND);
                }

                await ValidateDefaultValueAsync(command, targetEntity.Id, targetEntity.Columns);

                if (await _unitOfWork.Table.CheckExistNameAsync(command.Name, command.AssetId, command.Id))
                {
                    throw ValidationExceptionHelper.GenerateDuplicateValidation(nameof(Domain.Entity.Table.Name));
                }

                var hasData = await _unitOfWork.Table.CheckHasDataTableAsync(command.Id);

                foreach (var column in command.Columns.Where(col => col.Action == ColumnAction.NO))
                {
                    var exists = targetEntity.Columns.Any(x => x.Name == column.Name);
                    column.Action = exists ? ColumnAction.NO : ColumnAction.DELETE;
                }

                var requestEntity = UpdateTable.Create(command);
                UpdateTableColumns(requestEntity, targetEntity, command);

                var tableScriptBuilder = new UpdateTableScriptBuilder(TableDto.CreateUpdateTable(command), TableDto.Create(targetEntity, hasData));

                var scriptUpdateTable = tableScriptBuilder.Validate().BuildTable().GetScript();

                requestEntity.Script = scriptUpdateTable;

                command.Upn = _userContext.Upn;
                command.ApplicationId = Guid.Parse(!string.IsNullOrEmpty(_userContext.ApplicationId) ? _userContext.ApplicationId : ApplicationInformation.APPLICATION_ID);

                var isSameTag = command.IsSameTags(targetEntity.EntityTags);
                if (!isSameTag)
                {
                    await _unitOfWork.EntityTag.RemoveByEntityIdAsync(EntityTypeConstants.TABLE, targetEntity.Id, isTracking: true);

                    var tagIds = await _tagService.UpsertTagsAsync(command);
                    if (tagIds.Any())
                    {
                        var entitiesTags = tagIds.Distinct().Select(x => new EntityTagDb
                        {
                            EntityType = EntityTypeConstants.TABLE,
                            EntityIdGuid = command.Id,
                            TagId = x
                        }).ToArray();
                        await _unitOfWork.EntityTag.AddRangeWithSaveChangeAsync(entitiesTags);
                    }
                }
                await _unitOfWork.Table.UpdateTableAsync(requestEntity, targetEntity);
                await _unitOfWork.CommitAsync();
            }
            catch (System.Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Update, ex, command.Id, command.Name, command);
                throw;
            }

            // send unlock request
            if (entityLock != null &&
                entityLock.CurrentUserUpn == _userContext.Upn)
            {
                await _entityLockService.AcceptEntityUnlockRequestAsync(new EntityLock.Command.AcceptEntityUnlockRequestCommand()
                {
                    TargetId = command.Id
                }, cancellationToken);
            }

            var hashKey = CacheKey.TABLE_HASH_KEY.GetCacheKey(_tenantContext.ProjectId);
            await _cache.DeleteHashByKeyAsync(hashKey, CacheKey.TABLE_HASH_FIELD.GetCacheKey(command.Id));

            await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Update, ActionStatus.Success, command.Id, command.Name, command);

            return await _tagService.FetchTagsAsync(UpdateTableDto.Create(targetEntity));
        }

        private void UpdateTableColumns(Domain.Entity.Table requestEntity, Domain.Entity.Table targetEntity, UpdateTable command)
        {
            var addColumns = command.GetAddColumns().Select(x => UpdateColumn.Create(command.Id, x));
            var updateColumns = command.GetUpdateColumns().Select(x => UpdateColumn.Create(command.Id, x));
            var deleteColumns = command.GetDeleteColumns().Select(x => UpdateColumn.Create(command.Id, x));

            var columnList = targetEntity.Columns.ToList();
            // Add columns with action = "add"
            columnList.AddRange(addColumns);
            // Update columns with action = "update"
            foreach (var column in updateColumns)
            {
                // Just update columns are created by user
                var index = columnList.FindIndex(x => x.Id == column.Id && !column.IsSystemColumn);
                if (index != -1)
                    columnList[index] = column;
            }
            // Delete columns with action = "delete", just delete columns are created by user
            columnList = columnList.Where(x => !deleteColumns.Any(y => y.Id == x.Id) || x.IsSystemColumn).ToList();
            requestEntity.Columns = columnList;
        }

        public async Task<BaseSearchResponse<GetListTableDto>> GetTablesAsync(GetListTable command, CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            var query = _unitOfWork.Table.AsQueryable().AsNoTracking().Where(x => x.AssetId == command.AssetId);
            var hasFullAssetTableAccess = _securityService.HasFullAccessPrivilege(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME);
            if (!hasFullAssetTableAccess)
            {
                // if the user doesn't have full access permission -> need to query by created user.
                query = query.Where(x => x.CreatedBy == null || x.CreatedBy == _userContext.Upn);
            }
            var data = await query.OrderByDescending(x => x.UpdatedUtc).ToListAsync();
            var totalMiliseconds = (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds;
            return BaseSearchResponse<GetListTableDto>.CreateFrom(command, totalMiliseconds, data.Count, data.Select(GetListTableDto.Create));
        }

        public async Task<GetTableByIdDto> GetTableByIdAsync(GetTableById command, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.Table.FindTableAsync(command.Id);
            if (entity == null)
            {
                throw new EntityNotFoundException();
            }
            return await _tagService.FetchTagsAsync(GetTableByIdDto.Create(entity));
        }

        public override async Task<BaseSearchResponse<GetTableListDto>> SearchAsync(GetTableListByCriteria criteria)
        {
            criteria.MappingSearchTags();
            var response = await base.SearchAsync(criteria);
            return await _tagService.FetchTagsAsync(response);
        }

        public static Guid? GetRootElementId(Asset element)
        {
            if (element != null)
            {

                if (element.ParentAsset != null)
                {
                    return GetRootElementId(element.ParentAsset);
                }
                else
                {
                    return element.Id;
                }
            }
            return null;
        }

        public async Task<BaseResponse> DeleteTablesAsync(DeleteListTable command, CancellationToken cancellationToken)
        {
            var ids = command.Ids.Distinct().ToArray();
            var deleteTableNames = new List<string>();
            bool result = true;
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // check whether the table is locking or not
                var isLocking = await _entityLockService.ValidateEntitiesLockedByOtherAsync(new EntityLock.Command.ValidateLockEntitiesCommand(ids), cancellationToken);
                if (isLocking)
                {
                    // this entities has been locked.
                    throw new EntityAlreadyLockException();
                }

                var tables = await _unitOfWork.Table.AsQueryable().Where(x => command.Ids.Any(a => a == x.Id)).ToArrayAsync();
                if (ids.Length != tables.Length)
                {
                    throw new EntityNotFoundException(detailCode: ExceptionErrorCode.DetailCode.ERROR_ENTITY_NOT_FOUND_SOME_ITEMS_DELETED);
                }

                deleteTableNames.AddRange(tables.Select(x => x.Name));
                foreach (var item in tables)
                {
                    _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, item.ResourcePath, ownerUpn: item.CreatedBy, throwException: true);
                    _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.WRITE_ASSET_TABLE, item.ResourcePath, ownerUpn: item.CreatedBy, throwException: true);
                }

                result = await _unitOfWork.Table.DeleteTablesAsync(tables);

                await _unitOfWork.EntityTag.RemoveByEntityIdsAsync(EntityTypeConstants.TABLE, ids, isTracking: true);

                await _unitOfWork.CommitAsync();
            }
            catch (System.Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Delete, ex, command.Ids, deleteTableNames, command);
                throw;
            }

            var hashKey = CacheKey.TABLE_HASH_KEY.GetCacheKey(_tenantContext.ProjectId);
            var deleteFields = ids.Select(x =>
                            {
                                return CacheKey.TABLE_HASH_FIELD.GetCacheKey(x);
                            }).ToList();

            await _cache.DeleteHashByKeysAsync(hashKey, deleteFields);

            await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Delete, result ? ActionStatus.Success : ActionStatus.Fail, command.Ids, deleteTableNames, command);
            return new BaseResponse(result, string.Empty);
        }

        public async Task<BaseSearchResponse<object>> SearchAssetTableDataAsync(SearchAssetTableData command, CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            var entity = await _unitOfWork.Table.FindTableAsync(command.TableId);
            if (entity == null || entity.AssetId != command.AssetId)
            {
                throw new EntityNotFoundException();
            }
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, entity.ResourcePath, ownerUpn: entity.CreatedBy, throwException: true);
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.READ_ASSET_TABLE, entity.ResourcePath, ownerUpn: entity.CreatedBy, throwException: true);

            var tableName = GetTableName(entity.Id);

            var response = BaseSearchResponse<object>.CreateFrom(new BaseCriteria
            {
                PageIndex = command.PageIndex,
                PageSize = command.PageSize,
            }, 0, 0, new List<object>());
            var tasks = new Task[]{
                GetAssetTableDataAsync(command, tableName, response),
                GetAssetTableDataCountAsync(command, tableName, response)
            };
            await Task.WhenAll(tasks);

            response.DurationInMilisecond = (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds;

            return response;
        }

        private async Task GetAssetTableDataCountAsync(SearchAssetTableData command, string tableName, BaseSearchResponse<object> response)
        {
            var countSqlScript = $"select COUNT(1) from {tableName.ToStringQuote()}";
            var countValue = new ExpandoObject();

            var queryCriteria = command.ToQueryCriteria();
            queryCriteria.Sorts = null;
            (countSqlScript, countValue) = _queryService.CompileQuery(countSqlScript, queryCriteria, paging: false);

            var totalCount = await _unitOfWork.Table.CountTableDataAsync(countSqlScript, countValue);
            response.TotalCount = totalCount;
        }

        private async Task GetAssetTableDataAsync(SearchAssetTableData command, string tableName, BaseSearchResponse<object> response)
        {
            var pagingSqlScript = $"select * from {tableName.ToStringQuote()}";
            ExpandoObject pagingValue;

            var queryCriteria = command.ToQueryCriteria();
            (pagingSqlScript, pagingValue) = _queryService.CompileQuery(pagingSqlScript, queryCriteria);

            var items = await _unitOfWork.Table.GetTableDataAsync(pagingSqlScript, pagingValue);
            response.AddRangeData(items);
        }

        private async Task<TableDto> GetTableByIdAsync(Guid id)
        {
            var tableEntity = await _unitOfWork.Table.AsFetchable().Where(x => x.Id == id).FirstOrDefaultAsync();
            var tableDto = TableDto.Create(tableEntity);

            return tableDto;
        }

        public async Task<IEnumerable<object>> GetAssetTableDataAsync(GetAssetTableData command, CancellationToken cancellationToken)
        {
            var tableDto = await GetTableByIdAsync(command.Id);
            if (tableDto == null)
                throw new EntityNotFoundException();

            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, tableDto.ResourcePath, ownerUpn: tableDto.CreatedBy, throwException: true);
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.READ_ASSET_TABLE, tableDto.ResourcePath, ownerUpn: tableDto.CreatedBy, throwException: true);

            var tableName = GetTableName(tableDto.Id);
            var sqlScript = $"select * from {tableName.ToStringQuote()}";
            var value = new ExpandoObject();
            if (command.QueryCriteria != null)
            {
                (sqlScript, value) = _queryService.CompileQuery(sqlScript, command.QueryCriteria);
            }

            var data = await _unitOfWork.Table.GetTableDataAsync(sqlScript, value);
            return data;
        }

        public async Task<BaseResponse> UpsertAssetTableDataAsync(UpsertAssetTableData command, CancellationToken cancellationToken)
        {
            string tableName = null;
            try
            {
                var table = await _unitOfWork.Table.GetAssetTableByIdAsync(command.Id);
                if (table == null)
                {
                    throw new EntityNotFoundException();
                }
                tableName = table.TableName;
                _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, table.ResourcePath, ownerUpn: table.CreatedBy, throwException: true);
                _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.READ_ASSET_TABLE, table.ResourcePath, ownerUpn: table.CreatedBy, throwException: true);
                var columBuilder = new TableDataScriptBuilder(command, table);
                var createdBy = command.CallSource.IsSystem() ? "System" : _userContext.Upn.IsSystem() ? _userContext.Id.ToString() : _userContext.Upn;
                var result = columBuilder.BuildRows(createdBy).BuildScript();

                if (result.Errors.Any())
                {
                    throw new GenericCommonException(message: string.Join(",", result.Errors));
                }

                foreach (var queryResult in result.QueryResults)
                {
                    if (!string.IsNullOrEmpty(queryResult.Query))
                        await _unitOfWork.Table.UpsertTableDataAsync(queryResult.Query, queryResult.Value);
                }

                if (command.TrackActivity)
                    await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActivitiesLogEventAction.Insert, ActionStatus.Success, command.Id, table.TableName, command);

                return new BaseResponse(true, string.Empty);
            }
            catch (System.Exception ex)
            {
                if (command.TrackActivity)
                {
                    var messageCode = ex.Message;
                    if (ex is BaseException baseException)
                    {
                        messageCode = baseException.DetailCode;
                    }
                    await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActivitiesLogEventAction.Insert, ex, command.Id, tableName, new { Message = messageCode }, command);
                }
                _logger.LogError(ex, $"UpsertAssetTableDataAsync failed! {JsonConvert.SerializeObject(command)}");
                throw;
            }
        }

        public async Task<BaseResponse> UpsertTableDataAsync(UpsertTableData command, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.Table.FindAsync(command.Id);
            if (entity == null)
            {
                throw new EntityNotFoundException();
            }
            var upsertTableDataCommand = new UpsertAssetTableData(entity.AssetId.Value, command.Id, command.Data, false, ColumnAction.ADD, callSource: command.CallSource);

            return await UpsertAssetTableDataAsync(upsertTableDataCommand, cancellationToken);
        }

        public async Task<BaseResponse> DeleteTableDataAsync(DeleteTableData command, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.Table.FindTableAsync(command.Id);
            if (entity == null)
            {
                throw new EntityNotFoundException();
            }

            var primaryColumn = entity.Columns.First(x => x.IsPrimary);
            var data = command.Ids.Select(value => new Dictionary<string, object>() { [primaryColumn.Name] = value });
            var upsertTableDataCommand = new UpsertAssetTableData(entity.AssetId.Value, command.Id, data, false, ColumnAction.DELETE);
            var result = await UpsertAssetTableDataAsync(upsertTableDataCommand, default(CancellationToken));

            return result;
        }

        public async Task<object> AggregateTableDataAsync(AggregateTableData command, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.Table.FindAsync(command.Id);
            if (entity == null)
            {
                throw new EntityNotFoundException();
            }
            var assetTableAggregationBuilder = new AssetTableAggregationBuilder(command.ColumnName, command.AggregationCriteria, TableDto.Create(entity));
            var (query, value) = assetTableAggregationBuilder.BuildFilter().BuildQuery();
            var result = await _unitOfWork.Table.GetTableAggregationDataAsync(query, value);

            return result;
        }

        private async Task ValidateDefaultValueAsync(UpdateTable command, Guid tableId, IEnumerable<Column> columns)
        {
            var alterColumns = command.Columns.Where(x => x.Action == ColumnAction.ADD || x.Action == ColumnAction.UPDATE).ToList();
            if (!alterColumns.Any())
                return;
            var dataExists = await _unitOfWork.Table.CheckHasDataTableAsync(tableId);
            if (!dataExists)
                return;
            var columnsUpdate = new List<string>();
            foreach (var column in alterColumns)
            {
                if (column.Action == ColumnAction.ADD && !column.AllowNull && string.IsNullOrEmpty(column.DefaultValue))
                {
                    throw ValidationExceptionHelper.GenerateRequiredValidation(nameof(Column.DefaultValue));
                }
                else if (column.Action == ColumnAction.UPDATE && !column.AllowNull)
                {
                    var currentColumn = columns.FirstOrDefault(x => x.Id == column.Id);
                    if (currentColumn == null)
                        continue;
                    columnsUpdate.Add(currentColumn.Name);
                }
            }

            if (columnsUpdate.Any())
            {
                var existsData = await _unitOfWork.Table.CheckColumnHasNullDataAsync(tableId, columnsUpdate);
                if (existsData)
                {
                    throw ValidationExceptionHelper.GenerateRequiredValidation(nameof(Column.DefaultValue));
                }
            }
        }

        public async Task<TableDto> FetchTableByAssetAsync(Guid assetId, Guid tableId)
        {
            var table = await _unitOfWork.Table.AsFetchable().Where(x => x.Id == tableId && x.AssetId == assetId).FirstOrDefaultAsync();
            return TableDto.Create(table);
        }

        public static string GetTableName(Guid tableId)
        {
            return string.Format(TableName.PATTERN, tableId);
        }

        public async Task<ActivityResponse> ExportAssetTableAsync(ExportFile request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Table.AsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.TableId);
                if (entity == null)
                {
                    throw new EntityNotFoundException(detailCode: ExceptionErrorCode.DetailCode.ERROR_ENTITY_NOT_FOUND_SOME_ITEMS_DELETED);
                }

                await _fileEventService.SendExportEventAsync(request.ActivityId, entity.Id, entity.Name, request.Filter);

                return new ActivityResponse(request.ActivityId);
            }
            catch (System.Exception ex)
            {
                await _auditLogService.SendLogAsync(ActivityEntityAction.ASSET_TABLE, ActionType.Export, ex, payload: request);
                throw;
            }
        }

        public async Task<ArchiveAssetTableDto> ArchiveAsync(ArchiveAssetTable command, CancellationToken cancellation)
        {
            var result = new ArchiveAssetTableDto();
            var tables = await _unitOfWork.Table.AsQueryable().AsNoTracking().Include(x => x.Columns).Where(x => x.UpdatedUtc <= command.ArchiveTime).ToListAsync();
            var assetTables = await _unitOfWork.Table.ArchiveAsync(command.ArchiveTime);
            result.Tables = tables.Where(x => assetTables.Any(a => a.TableName == string.Format(TableName.PATTERN, x.Id))).Select(x => ArchiveTableDto.CreateDto(x));
            result.AssetTables = assetTables.Where(x => tables.Any(t => x.TableName == string.Format(TableName.PATTERN, t.Id)));

            return result;
        }

        public async Task<BaseResponse> RetrieveAsync(RetrieveAssetTable command, CancellationToken cancellation)
        {
            _userContext.SetUpn(command.Upn);
            var data = JsonConvert.DeserializeObject<ArchiveAssetTableDto>(command.Data, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            var entities = data.Tables.OrderBy(x => x.UpdatedUtc).Select(x => ArchiveTableDto.CreateEntity(x, command.Upn));
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Table.RetrieveAsync(entities);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            await _unitOfWork.Table.GenerateTablesAsync(entities, data.AssetTables);
            return BaseResponse.Success;
        }

        public async Task<BaseResponse> VerifyArchiveAsync(VerifyAssetTable command, CancellationToken cancellation)
        {
            var data = JsonConvert.DeserializeObject<ArchiveAssetTableDto>(command.Data, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            var validation = await _validator.ValidateAsync(data, cancellation);

            if (!validation.IsValid)
                throw EntityValidationExceptionHelper.GenerateException(nameof(command.Data), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);

            if (data.AssetTables.Any(x => !data.Tables.Any(t => x.TableName == string.Format(TableName.PATTERN, t.Id))))
                throw EntityValidationExceptionHelper.GenerateException(nameof(command.Data), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);

            return BaseResponse.Success;
        }
    }
}
