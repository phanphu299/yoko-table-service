using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.AssetTable.Command;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.FileRequest.Command;
using AssetTable.Application.Models;
using AssetTable.Application.TableList.Command;
using AssetTable.Application.TableList.Command.Model;

namespace AssetTable.Application.Service.Abstraction
{
    public interface ITableService : ISearchService<Domain.Entity.Table, Guid, GetTableListByCriteria, GetTableListDto>, IFetchService<Domain.Entity.Table, Guid, GetTableListDto>
    {
        Task<GetTableListDto> FindAssetTableByIdAsync(GetTableListById command, CancellationToken token);
        Task<BaseResponse> RemoveAssetTablesAsync(DeleteTableList command, CancellationToken token);
        Task<BaseResponse> ValidateExistAssetTablesAsync(CheckExistingTableList command, CancellationToken token);
        Task<AddTableDto> AddTableAsync(AddTable command, CancellationToken cancellationToken);
        Task<UpdateTableDto> UpdateTableAsync(UpdateTable command, CancellationToken cancellationToken);
        Task<AHI.Infrastructure.SharedKernel.Model.BaseSearchResponse<GetListTableDto>> GetTablesAsync(GetListTable command, CancellationToken cancellationToken);
        Task<GetTableByIdDto> GetTableByIdAsync(GetTableById command, CancellationToken cancellationToken);
        Task<BaseResponse> DeleteTablesAsync(DeleteListTable command, CancellationToken cancellationToken);
        Task<IEnumerable<object>> GetAssetTableDataAsync(GetAssetTableData command, CancellationToken cancellationToken);
        Task<BaseSearchResponse<object>> SearchAssetTableDataAsync(SearchAssetTableData command, CancellationToken cancellationToken);
        Task<BaseResponse> UpsertAssetTableDataAsync(UpsertAssetTableData command, CancellationToken cancellationToken);
        Task<object> AggregateTableDataAsync(AggregateTableData command, CancellationToken cancellationToken);
        Task<BaseResponse> UpsertTableDataAsync(UpsertTableData command, CancellationToken cancellationToken);
        Task<BaseResponse> DeleteTableDataAsync(DeleteTableData command, CancellationToken cancellationToken);
        Task<TableDto> FetchTableByAssetAsync(Guid assetId, Guid tableId);
        Task<ActivityResponse> ExportAssetTableAsync(ExportFile request, CancellationToken cancellationToken);
        Task<ArchiveAssetTableDto> ArchiveAsync(ArchiveAssetTable command, CancellationToken cancellation);
        Task<BaseResponse> VerifyArchiveAsync(VerifyAssetTable command, CancellationToken cancellation);
        Task<BaseResponse> RetrieveAsync(RetrieveAssetTable command, CancellationToken cancellation);
    }
}
