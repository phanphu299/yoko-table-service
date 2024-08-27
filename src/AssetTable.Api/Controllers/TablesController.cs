using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AHI.Infrastructure.Authorization;
using System;
using AssetTable.Application.Constant;
using AssetTable.Application.AssetTable.Command;
using System.Collections.Generic;
using AHI.Infrastructure.Service.Dapper.Model;
using AssetTable.Application.TableList.Command;
using AssetTable.Application.Service;
using AssetTable.Application.FileRequest.Command;

namespace AssetTable.Api.Controller
{
    // file deepcode ignore AntiforgeryTokenDisabled: Ignore ValidateAntiForgeryToken because we have RightsAuthorizeFilter
    [Route("tbl/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "oidc")]
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Function Block will call to this API
        /// </summary>
        [HttpPost("{id}/query")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> GetTableDataAsync([FromRoute] Guid id, QueryCriteria queryCriteria)
        {
            var command = new GetTableData(id, queryCriteria);
            var response = await _mediator.Send(command);

            // special case, ignore camel case property naming policy cause it has table column names
            byte[] jsonUtf8Bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(response);
            return File(jsonUtf8Bytes, "application/json");
        }

        [HttpPost("{id}/upsert")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> UpsertTableDataAsync([FromRoute] Guid id, [FromQuery] string callSource, [FromBody] IEnumerable<IDictionary<string, object>> data)
        {
            var command = new UpsertTableData(id, data, callSource: callSource);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("{id}/delete")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.DELETE_ASSET_TABLE)]
        public async Task<IActionResult> DeleteTableDataAsync([FromRoute] Guid id, [FromBody] DeleteTableData command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("import")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> ImportAsync([FromBody] ImportFile command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAsync([FromBody] ExportFile command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("{id}/aggregate")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> AggregateAsync([FromRoute] Guid id, AggregateTableData command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("asset/{assetId}/tables")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> AddTableAsync([FromRoute] Guid assetId, [FromBody] AddTable command)
        {
            command.AssetId = assetId;
            var response = await _mediator.Send(command);
            return CreatedAtAction("GetTableById", new { assetId = command.AssetId, tableId = response.Id }, response);
        }

        [HttpPut("asset/{assetId}/tables/{tableId}")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> UpdateTableAsync([FromRoute] Guid assetId, [FromRoute] Guid tableId, [FromBody] UpdateTable command)
        {
            command.AssetId = assetId;
            command.Id = tableId;
            var response = await _mediator.Send(command);
            return AcceptedAtAction("GetTableById", new { assetId = assetId, tableId = tableId }, response);
        }

        [HttpGet("asset/{assetId}/tables")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> GetTablesAsync([FromRoute] Guid assetId)
        {
            var command = new GetListTable(assetId);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [Obsolete("will be remove")]
        [HttpGet("asset/{assetId}/tables/{tableId}", Name = "GetTableByAssetIdAsync")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> GetTableByAssetIdAsync([FromRoute] Guid assetId, [FromRoute] Guid tableId)
        {
            var command = new GetTableById(tableId);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("tables/{tableId}", Name = "GetTableById")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> GetTableByIdAsync([FromRoute] Guid tableId)
        {
            var command = new GetTableById(tableId);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("asset/{assetId}/tables")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.DELETE_ASSET_TABLE)]
        public async Task<IActionResult> DeleteTablesAsync([FromRoute] Guid assetId, [FromBody] IEnumerable<Guid> ids)
        {
            var command = new DeleteListTable(assetId, ids);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [Obsolete("This API is no longer in use")]
        [HttpGet("asset/{assetId}/tables/{tableId}/query")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> GetTableDataAsync([FromRoute] Guid assetId, [FromRoute] Guid tableId)
        {
            var command = new GetAssetTableData(tableId);
            var response = await _mediator.Send(command);

            // special case, ignore camel case property naming policy cause it has table column names
            byte[] jsonUtf8Bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(response);
            return File(jsonUtf8Bytes, "application/json");
        }

        [HttpPost("asset/{assetId}/tables/{tableId}/search")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> SearchAssetTableDataAsync(
            [FromRoute] Guid assetId,
            [FromRoute] Guid tableId,
            [FromBody] SearchAssetTableData command
            )
        {
            command.AssetId = assetId;
            command.TableId = tableId;
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("asset/{assetId}/tables/{tableId}/insert")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> UpsertTableDataAsync([FromRoute] Guid assetId, [FromRoute] Guid tableId, [FromBody] IEnumerable<IDictionary<string, object>> data)
        {
            var command = new UpsertAssetTableData(assetId, tableId, data, trackActivity: true, isUpsert: false);
            var response = await _mediator.Send(command);
            return Ok(response);
        }


        [HttpPost("asset/{assetId}/tables/search")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> SearchTableList(Guid assetId, [FromBody] GetTableListByCriteria command)
        {
            command.AssetId = assetId;
            command.IncludeLockInformation = true;
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        [HttpGet("asset/{assetId}/tables/{tableId}/fetch")]
        public async Task<IActionResult> FetchTableAsync(Guid assetId, Guid tableId)
        {
            var command = new FetchAssetTable(tableId, assetId);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("archive")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> ArchiveAsync([FromBody] ArchiveAssetTable command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("archive/verify")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.READ_ASSET_TABLE)]
        public async Task<IActionResult> VerifyArchiveAsync([FromBody] VerifyAssetTable command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("retrieve")]
        [RightsAuthorizeFilterAttribute(Privileges.AssetTable.FullRights.WRITE_ASSET_TABLE)]
        public async Task<IActionResult> RetrieveAsync([FromBody] RetrieveAssetTable command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}