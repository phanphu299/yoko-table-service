
using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.TableList.Command.Model;

namespace AssetTable.Application.TableList.Command.Handler
{
    public class FetchTableRequestHandler : IRequestHandler<FetchTable, GetTableListDto>
    {
        private readonly ITableService _service;

        public FetchTableRequestHandler(ITableService service)
        {
            _service = service;
        }

        public Task<GetTableListDto> Handle(FetchTable request, CancellationToken cancellationToken)
        {
            return _service.FetchAsync(request.Id);
        }
    }
}