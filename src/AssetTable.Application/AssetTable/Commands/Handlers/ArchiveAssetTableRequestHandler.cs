using System.Threading;
using System.Threading.Tasks;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Service.Abstraction;
using MediatR;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class ArchiveAssetTableRequestHandler : IRequestHandler<ArchiveAssetTable, ArchiveAssetTableDto>
    {
        private readonly ITableService _tableService;

        public ArchiveAssetTableRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public Task<ArchiveAssetTableDto> Handle(ArchiveAssetTable request, CancellationToken cancellationToken)
        {
            return _tableService.ArchiveAsync(request, cancellationToken);
        }
    }
}
