using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class AgregateTableValueRequestHandler : IRequestHandler<AggregateTableData, object>
    {
        private readonly ITableService _service;

        public AgregateTableValueRequestHandler(ITableService service)
        {
            _service = service;
        }

        public async Task<object> Handle(AggregateTableData request, CancellationToken cancellationToken)
        {
            return await _service.AggregateTableDataAsync(request, cancellationToken);
        }
    }
}