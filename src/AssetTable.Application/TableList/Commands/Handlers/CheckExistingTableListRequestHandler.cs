
using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Service.Abstraction;

namespace AssetTable.Application.TableList.Command.Handler
{
    public class CheckExistingTableListRequestHandler : IRequestHandler<CheckExistingTableList, BaseResponse>
    {
        private readonly ITableService _service;
        public CheckExistingTableListRequestHandler(ITableService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(CheckExistingTableList request, CancellationToken cancellationToken)
        {
            return _service.ValidateExistAssetTablesAsync(request, cancellationToken);
        }
    }
}