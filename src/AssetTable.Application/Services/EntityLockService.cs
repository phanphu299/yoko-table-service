using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.UserContext.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using System.Collections.Generic;
using AHI.Infrastructure.Exception;
using System.Net.Http;
using AHI.Infrastructure.SharedKernel.Extension;
using Newtonsoft.Json;
using AHI.Infrastructure.MultiTenancy.Extension;
using AssetTable.Application.Service.Abstraction;
using AssetTable.Application.EntityLock.Command;
using AssetTable.Application.Constant;
using AssetTable.Application.Command.Model;

namespace AssetTable.Application.Service
{
    public class EntityLockService : IEntityLockService
    {
        private readonly IUserContext _userContext;
        private readonly ITenantContext _tenantContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public EntityLockService(
            IUserContext userContext,
            ITenantContext tenantContext,
            IHttpClientFactory httpClientFactory)
        {
            _userContext = userContext;
            _tenantContext = tenantContext;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> AcceptEntityUnlockRequestAsync(AcceptEntityUnlockRequestCommand command, CancellationToken token)
        {
            var entityService = _httpClientFactory.CreateClient(HttpClientNames.ENTITY_SERVICE, _tenantContext);
            var responseMessage = await entityService.PostAsync($"ent/locks/{command.TargetId}/lock/request/release/accept", new StringContent(JsonConvert.SerializeObject(new
            {
                RequestLockUpn = _userContext.Upn,
                Timeout = 0
            }), System.Text.Encoding.UTF8, "application/json"));
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsByteArrayAsync();
                var accepted = responseData.Deserialize<BaseResponse>();
                return accepted;
            }
            return BaseResponse.Failed;
        }

        public async Task<EntityLockDto> GetLockEntityAsync(Guid entityId, CancellationToken token)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.ENTITY_SERVICE, _tenantContext);
            var response = await httpClient.GetAsync($"ent/locks/{entityId}/lock");
            if (!response.IsSuccessStatusCode)
                return null;

            var responseData = await response.Content.ReadAsByteArrayAsync();
            return responseData.Deserialize<EntityLockDto>();
        }

        public async Task<bool> ValidateEntitiesLockedByOtherAsync(ValidateLockEntitiesCommand command, CancellationToken token)
        {
            var entityIds = command.TargetIds.ToArray();
            var entities = await GetLockEntitesAsync(entityIds, command.HolderUpn);
            return entities.Any();
        }

        private async Task<IEnumerable<Guid>> GetLockEntitesAsync(IEnumerable<Guid> entityIds, string upn)
        {
            var entityService = _httpClientFactory.CreateClient(HttpClientNames.ENTITY_SERVICE, _tenantContext);
            var responseMessage = await entityService.PostAsync($"ent/locks/lock/entities", new StringContent(JsonConvert.SerializeObject(new
            {
                TargetIds = entityIds,
                HolderUpn = upn
            }), System.Text.Encoding.UTF8, "application/json"));
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsByteArrayAsync();
                var lockedEntityIds = responseData.Deserialize<IEnumerable<Guid>>();
                return lockedEntityIds;
            }
            else
            {
                throw new SystemCallServiceException();
            }
        }
    }
}
