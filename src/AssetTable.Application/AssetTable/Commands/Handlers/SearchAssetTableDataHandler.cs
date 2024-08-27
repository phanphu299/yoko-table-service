using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class SearchAssetTableDataHandler : IRequestHandler<SearchAssetTableData, BaseSearchResponse<object>>
    {
        private readonly Service.Abstraction.ITableService _service;
        
        public SearchAssetTableDataHandler(
            Service.Abstraction.ITableService service
        )
        {
            _service = service;
        }

        public async Task<BaseSearchResponse<object>> Handle(SearchAssetTableData request, CancellationToken cancellationToken)
        {
            return await _service.SearchAssetTableDataAsync(request, cancellationToken);
        }
    }
}