
using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Service.Abstraction;

namespace AssetTable.Application.TableList.Command.Handler
{
    public class DeleteTableListRequestHandler : IRequestHandler<DeleteTableList, BaseResponse>
    {
        private readonly ITableService _service;
        public DeleteTableListRequestHandler(ITableService service)
        {
            _service = service;
        }

        public async Task<BaseResponse> Handle(DeleteTableList request, CancellationToken cancellationToken)
        {
            return await _service.RemoveAssetTablesAsync(request, cancellationToken);
        }
    }
}