
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AHI.Infrastructure.Authorization;
using System;
using AssetTable.Application.Constant;
using AssetTable.Application.TableList.Command;

namespace AssetTable.Api.Controller
{
    // file deepcode ignore AntiforgeryTokenDisabled: Ignore ValidateAntiForgeryToken because we have RightsAuthorizeFilter
    [Route("tbl/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "oidc")]
    public class TableListController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TableListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search")]
        // [RightsAuthorizeFilterAttribute(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, "tbl/tablelist/search", Privileges.AssetTable.Rights.READ_ASSET_TABLE)]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> SearchTableList([FromBody] GetTableListByCriteria command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete]
        // [RightsAuthorizeFilterAttribute(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, "tbl/tablelist", Privileges.AssetTable.Rights.DELETE_ASSET_TABLE)]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.DELETE_ASSET_TABLE)]
        public async Task<IActionResult> DeleteListTableListAsync([FromBody] DeleteTableList command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpHead("{id}")]
        // [RightsAuthorizeFilterAttribute(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, "tbl/tablelist/{id}", Privileges.AssetTable.Rights.READ_ASSET_TABLE)]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> CheckExistingTableListAsync([FromRoute] Guid id)
        {
            var command = new CheckExistingTableList(new[] { id });
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("exist")]
        // [RightsAuthorizeFilterAttribute(ApplicationInformation.APPLICATION_ID, Privileges.AssetTable.ENTITY_NAME, "tbl/tablelist/exist", Privileges.AssetTable.Rights.READ_ASSET_TABLE)]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> CheckExistingTableListAsync([FromBody] CheckExistingTableList command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("{id}/fetch")]
        public async Task<IActionResult> FetchAsync(Guid id)
        {
            var command = new FetchTable(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}
