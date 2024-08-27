using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Service.Abstraction;
using MediatR;

namespace AssetTable.Application.FileRequest.Command.Handler
{
    public class ImportFileRequestHandler : IRequestHandler<ImportFile, BaseResponse>
    {
        private readonly IFileEventService _fileEventService;
        private readonly ITableService _tableService;
        public ImportFileRequestHandler(IFileEventService fileEventService, ITableService tableService)
        {
            _fileEventService = fileEventService;
            _tableService = tableService;
        }

        public async Task<BaseResponse> Handle(ImportFile request, CancellationToken cancellationToken)
        {
            var tableDto = await _tableService.GetTableByIdAsync(new AssetTable.Command.GetTableById(request.TableId), cancellationToken);
            await _fileEventService.SendImportEventAsync(tableDto.Id, tableDto.Name, request.FileNames);
            return BaseResponse.Success;
        }

    }
}
