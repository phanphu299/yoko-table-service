using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class UpsertAssetTableDataRequestHandler : IRequestHandler<UpsertAssetTableData, BaseResponse>
    {
        private readonly ITableService _service;

        public UpsertAssetTableDataRequestHandler(ITableService service)
        {
            _service = service;
        }

        public async Task<BaseResponse> Handle(UpsertAssetTableData request, CancellationToken cancellationToken)
        {
            return await _service.UpsertAssetTableDataAsync(request, cancellationToken);
        }
    }
}