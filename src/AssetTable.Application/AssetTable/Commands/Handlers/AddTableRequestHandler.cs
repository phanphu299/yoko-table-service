using System.Threading;
using MediatR;
using System.Threading.Tasks;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Constant;
using AHI.Infrastructure.UserContext.Abstraction;
using AHI.Infrastructure.UserContext.Service.Abstraction;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class AddTableRequestHandler : IRequestHandler<AddTable, AddTableDto>
    {
        private readonly ITableService _tableService;
        private readonly ISecurityService _securityService;
        private readonly IAssetService _assetService;
        private readonly IUserContext _userContext;
        public AddTableRequestHandler(ITableService tableService, ISecurityService securityService, IAssetService assetService, IUserContext userContext)
        {
            _tableService = tableService;
            _securityService = securityService;
            _assetService = assetService;
            _userContext = userContext;
        }

        public async Task<AddTableDto> Handle(AddTable request, CancellationToken cancellationToken)
        {
            var assetDto = await _assetService.FetchAsync(request.AssetId, cancellationToken);
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, assetDto.ResourcePath, _userContext.Upn, true);
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.WRITE_ASSET_TABLE, assetDto.ResourcePath, ownerUpn: _userContext.Upn, true);

            request.ResourcePath = assetDto.ResourcePath;
            request.AssetName = assetDto.Name;
            request.CreatedBy = _userContext.Upn;
            request.AssetCreatedBy = assetDto.CreatedBy;
            return await _tableService.AddTableAsync(request, cancellationToken);
        }
    }
}

