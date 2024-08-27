using System;
using AHI.Infrastructure.Service.Dapper.Extensions;
using AHI.Infrastructure.Service.Dapper.Helpers;
using AHI.Infrastructure.Service.Dapper.Model;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.SharedKernel.Models;
using AssetTable.Application.Constant;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AssetTable.Application.AssetTable.Command
{
    public class SearchAssetTableData : BaseCriteria, IRequest<BaseSearchResponse<object>>
    {
        public Guid AssetId { get; set; }
        public Guid TableId { get; set; }
        public bool ClientOverride { get; set; } = false;

        public SearchAssetTableData()
        {
            PageSize = 20;
            PageIndex = 0;
            Sorts = DefaultSearchConstants.DEFAULT_SORT;
        }

        public QueryCriteria ToQueryCriteria()
        {
            var queryCriteria = new QueryCriteria
            {
                Filter = !string.IsNullOrEmpty(Filter) ? JsonConvert.DeserializeObject<JObject>(Filter) : null,
                PageIndex = PageIndex,
                PageSize = PageSize,
                Sorts = Sorts
            };

            if (!string.IsNullOrEmpty(Filter))
            {
                queryCriteria.Filter = DynamicCriteriaHelper.ProcessDapperQueryFilter(Filter, queryCriteria.Filter, queryCriteria, ConvertSearchFilter);
            }
            if (!string.IsNullOrEmpty(Sorts))
            {
                queryCriteria.Sorts = DynamicCriteriaHelper.ProcessDapperQuerySort(Sorts, queryCriteria, ConvertSortKey);
            }
            return queryCriteria;
        }

        private SearchFilter ConvertSearchFilter(SearchFilter condition, QueryCriteria queryCriteria)
        {
            condition.QueryKey = condition.QueryKey.ConvertQueryKeyToLower();
            return condition;
        }

        private string ConvertSortKey(string key, string order, QueryCriteria queryCriteria)
        {
            return key.ConvertQueryKeyToLower();
        }
    }
}