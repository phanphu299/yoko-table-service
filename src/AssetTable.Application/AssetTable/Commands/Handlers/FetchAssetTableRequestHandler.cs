using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Service;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class FetchAssetTableRequestHandler : IRequestHandler<FetchAssetTable, TableDto>
    {
        private readonly ITableService _service;

        public FetchAssetTableRequestHandler(ITableService service)
        {
            _service = service;
        }

        public Task<TableDto> Handle(FetchAssetTable request, CancellationToken cancellationToken)
        {
            return _service.FetchTableByAssetAsync(request.AssetId, request.Id);
        }
    }
}
