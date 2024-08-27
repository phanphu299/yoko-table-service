using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.SharedKernel.Models;
using AHI.Infrastructure.UserContext.Service.Abstraction;
using AssetTable.Application.Constant;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.TableList.Command.Model;
using MediatR;
using Newtonsoft.Json;

namespace AssetTable.Application.TableList.Command.Handler
{
    public class GetTableListByCriteriaRequestHandler : IRequestHandler<GetTableListByCriteria, BaseSearchResponse<GetTableListDto>>
    {
        private readonly ITableService _service;
        private readonly ISecurityContext _securityContext;

        public GetTableListByCriteriaRequestHandler(ITableService service, ISecurityContext securityContext)
        {
            _service = service;
            _securityContext = securityContext;
        }

        public Task<BaseSearchResponse<GetTableListDto>> Handle(GetTableListByCriteria request, CancellationToken cancellationToken)
        {
            _securityContext.Authorize(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, Privileges.AssetTable.Rights.READ_ASSET_TABLE);
            if (request.AssetId != null)
            {
                var filters = new List<SearchFilter>
                {
                    new SearchFilter("assetId", request.AssetId.ToString(), queryType: "guid")
                };
                var finalFilter = new SearchAndFilter(filters, request.Filter);
                request.Filter = JsonConvert.SerializeObject(finalFilter);
            }
            return _service.RelationSearchWithSecurityAsync(request, objectKeyName: "assetId");
        }
    }
}