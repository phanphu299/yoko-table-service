using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Service.Abstraction;
using MediatR;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class RetrieveAssetTableRequestHandler : IRequestHandler<RetrieveAssetTable, BaseResponse>
    {
        private readonly ITableService _tableService;

        public RetrieveAssetTableRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public Task<BaseResponse> Handle(RetrieveAssetTable request, CancellationToken cancellationToken)
        {
            return _tableService.RetrieveAsync(request, cancellationToken);
        }
    }
}
