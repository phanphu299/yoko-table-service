using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Service.Abstraction;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class VerifyArchiveAssetTableRequestHandler : IRequestHandler<VerifyAssetTable, BaseResponse>
    {
        private readonly ITableService _tableService;

        public VerifyArchiveAssetTableRequestHandler(ITableService tableService)
        {
            _tableService = tableService;
        }

        public Task<BaseResponse> Handle(VerifyAssetTable request, CancellationToken cancellationToken)
        {
            return _tableService.VerifyArchiveAsync(request, cancellationToken);
        }
    }
}
