using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class GetAssetTableDataRequestHandler : IRequestHandler<GetAssetTableData, IEnumerable<object>>
    {
        private readonly ITableService _tableService;
        public GetAssetTableDataRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task<IEnumerable<object>> Handle(GetAssetTableData request, CancellationToken cancellationToken)
        {
            var details = await _tableService.GetAssetTableDataAsync(request, cancellationToken);
            return details.Select(x => JObject.FromObject(x).ToObject<Dictionary<string, string>>());
        }
    }
}