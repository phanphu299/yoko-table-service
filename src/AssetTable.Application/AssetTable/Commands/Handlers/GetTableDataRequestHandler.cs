using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class GetTableDataRequestHandler : IRequestHandler<GetTableData, IEnumerable<object>>
    {
        private readonly ITableService _service;
        public GetTableDataRequestHandler(ITableService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<object>> Handle(GetTableData request, CancellationToken cancellationToken)
        {
            var command = new GetAssetTableData(request.Id, request.QueryCriteria);
            var details = await _service.GetAssetTableDataAsync(command, cancellationToken);
            return details.Select(x => JObject.FromObject(x).ToObject<Dictionary<string, string>>());
        }
    }
}