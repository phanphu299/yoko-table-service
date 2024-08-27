using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Constant;
using AssetTable.Application.TableList.Command.Model;
using MediatR;
using System;
using System.Security.AccessControl;
using System.Text.Json.Serialization;

namespace AssetTable.Application.TableList.Command
{
    public class GetTableListByCriteria : BaseCriteria, IRequest<BaseSearchResponse<GetTableListDto>>
    {
        // public bool ClientOverride { get; set; } = false;
        public Guid? AssetId { get; set; }
        [JsonIgnore]
        public ObjectSecurity ObjectSecurity { get; set; }
        public bool IncludeLockInformation { get; set; }
        public GetTableListByCriteria()
        {
            PageSize = 20;
            PageIndex = 0;
            Sorts = DefaultSearchConstants.DEFAULT_SORT;
        }
    }
}
