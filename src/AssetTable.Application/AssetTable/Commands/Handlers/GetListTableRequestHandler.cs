using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.UserContext.Abstraction;
using AHI.Infrastructure.UserContext.Service.Abstraction;

using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Constant;
using AssetTable.Application.Service.Abstraction;

using MediatR;

namespace AssetTable.Application.AssetTable.Command.Handler
{
    public class GetListTableRequestHandler : IRequestHandler<GetListTable, BaseSearchResponse<GetListTableDto>>
    {
        private readonly ITableService _tableService;
        private readonly IAssetService _assetService;
        private readonly ISecurityService _securityService;
        private readonly IUserContext _userContext;

        public GetListTableRequestHandler(ITableService tableService, ISecurityService securityService, IUserContext userContext, IAssetService assetService)
        {
            _tableService = tableService;
            _securityService = securityService;
            _userContext = userContext;
            _assetService = assetService;
        }

        public async Task<BaseSearchResponse<GetListTableDto>> Handle(GetListTable request, CancellationToken cancellationToken)
        {
            var asset = await _assetService.FetchAsync(request.AssetId, cancellationToken);
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.Asset.ENTITY_NAME, Privileges.Asset.Rights.READ_ASSET, asset.ResourcePath, ownerUpn: asset.CreatedBy);
            _securityService.AuthorizeAccess(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.READ_ASSET_TABLE, asset.ResourcePath, ownerUpn: asset.CreatedBy);
            return await _tableService.GetTablesAsync(request, cancellationToken);
        }
    }
}
