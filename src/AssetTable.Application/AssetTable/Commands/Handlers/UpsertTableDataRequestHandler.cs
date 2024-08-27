using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class UpsertTableDataRequestHandler : IRequestHandler<UpsertTableData, BaseResponse>
    {
        private readonly ITableService _service;

        public UpsertTableDataRequestHandler(ITableService service)
        {
            _service = service;
        }

        public async Task<BaseResponse> Handle(UpsertTableData request, CancellationToken cancellationToken)
        {
            return await _service.UpsertTableDataAsync(request, cancellationToken);
        }
    }
}