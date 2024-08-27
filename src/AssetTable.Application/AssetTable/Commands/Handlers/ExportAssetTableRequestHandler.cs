using System.Threading;
using System.Threading.Tasks;
using AssetTable.Application.FileRequest.Command;
using AssetTable.Application.Models;
using AssetTable.Application.Service.Abstraction;
using MediatR;

namespace Device.Application.AssetTemplate.Command.Handler
{
    public class ExportAssetTableRequestHandler : IRequestHandler<ExportFile, ActivityResponse>
    {
        private readonly ITableService _service;

        public ExportAssetTableRequestHandler(ITableService service)
        {
            _service = service;
        }

        public Task<ActivityResponse> Handle(ExportFile request, CancellationToken cancellationToken)
        {
            return _service.ExportAssetTableAsync(request, cancellationToken);
        }
    }
}
