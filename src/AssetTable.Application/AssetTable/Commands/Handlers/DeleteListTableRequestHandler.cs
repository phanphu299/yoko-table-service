using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class DeleteListTableRequestHandler : IRequestHandler<DeleteListTable, BaseResponse>
    {
        private readonly ITableService _tableService;

        public DeleteListTableRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task<BaseResponse> Handle(DeleteListTable request, CancellationToken cancellationToken)
        {
            return await _tableService.DeleteTablesAsync(request, cancellationToken);
        }
    }
}
