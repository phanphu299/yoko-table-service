using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class UpdateTableRequestHandler : IRequestHandler<UpdateTable, UpdateTableDto>
    {
        private readonly ITableService _tableService;
        public UpdateTableRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task<UpdateTableDto> Handle(UpdateTable request, CancellationToken cancellationToken)
        {
            return await _tableService.UpdateTableAsync(request, cancellationToken);
        }
    }
}