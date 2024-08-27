using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;
using AHI.Infrastructure.Repository.Generic;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.Repository
{
    public interface ITableRepository : IRepository<Domain.Entity.Table, Guid>
    {
        Task<Domain.Entity.Table> FindTableAsync(Guid id);
        Task<bool> CheckExistNameAsync(string name, Guid assetId, Guid? id = null);
        Task<Domain.Entity.Table> AddTableAsync(Domain.Entity.Table entity);
        Task<Domain.Entity.Table> UpdateTableAsync(Domain.Entity.Table requestEntity, Domain.Entity.Table targetEntity);
        Task<bool> DeleteTablesAsync(IEnumerable<Domain.Entity.Table> tables);
        Task<IEnumerable<object>> GetTableDataAsync(string query, ExpandoObject value = null);
        Task<int> CountTableDataAsync(string query, ExpandoObject value = null);
        Task UpsertTableDataAsync(string upsertQuery, ExpandoObject value = null);
        Task<object> GetTableAggregationDataAsync(string aggregationQuery, object value);
        Task<bool> CheckHasDataTableAsync(Guid tableId);
        Task<bool> CheckColumnHasNullDataAsync(Guid tableId, IEnumerable<string> columns);
        Task<AssetTableDto> GetAssetTableByIdAsync(Guid id);
        Task<bool> RemoveListEntityWithRelationAsync(ICollection<Domain.Entity.Table> AssetTables);
        Task<IEnumerable<ArchiveAssetDto>> ArchiveAsync(DateTime archiveTime);
        Task RetrieveAsync(IEnumerable<Domain.Entity.Table> tables);
        Task GenerateTablesAsync(IEnumerable<Domain.Entity.Table> tables, IEnumerable<ArchiveAssetDto> assetTables);
    }
}
