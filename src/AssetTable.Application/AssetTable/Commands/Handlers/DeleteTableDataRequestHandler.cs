using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class DeleteTableDataRequestHandler : IRequestHandler<DeleteTableData, BaseResponse>
    {
        private readonly ITableService _service;

        public DeleteTableDataRequestHandler(ITableService service)
        {
            _service = service;
        }

        public async Task<BaseResponse> Handle(DeleteTableData request, CancellationToken cancellationToken)
        {
            return await _service.DeleteTableDataAsync(request, cancellationToken);
        }
    }
}