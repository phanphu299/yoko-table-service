using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class GetTableRequestHandler : IRequestHandler<GetTableById, GetTableByIdDto>
    {
        private readonly ITableService _tableService;
        public GetTableRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task<GetTableByIdDto> Handle(GetTableById request, CancellationToken cancellationToken)
        {
            return await _tableService.GetTableByIdAsync(request, cancellationToken);
        }
    }
}
