using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.SharedKernel.Extension;
using AssetTable.Application.AssetTable.Command.Model;
using AssetTable.Application.Constant;
using AssetTable.Application.Service.Abstraction;
using AssetTable.ApplicationExtension.Extension;

namespace AssetTable.Application.Service
{
    public class AssetService : IAssetService
    {
        private readonly ITenantContext _tenantContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICache _cache;

        public AssetService(ITenantContext tenantContext,
                    IHttpClientFactory httpClientFactory,
                    ICache cache)
        {
            _tenantContext = tenantContext;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<SimpleAssetDto> FetchAsync(Guid assetId, CancellationToken token)
        {
            var hashField = CacheKey.ASSET_HASH_FIELD.GetCacheKey(assetId);
            var hashKey = CacheKey.ASSET_HASH_KEY.GetCacheKey(_tenantContext.ProjectId);

            SimpleAssetDto asset = await _cache.GetHashByKeyAsync<SimpleAssetDto>(hashKey, hashField);

            if (asset == null)
            {
                var client = _httpClientFactory.CreateClient(HttpClientNames.DEVICE, _tenantContext);
                var responseMessage = await client.GetAsync($"dev/assets/{assetId}/fetch", token);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new EntityNotFoundException(detailCode: MessageConstants.ASSET_NOT_FOUND);
                    }
                    throw new SystemCallServiceException();
                }
                else if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new EntityNotFoundException(detailCode: MessageConstants.ASSET_NOT_FOUND);
                }

                var responseData = await responseMessage.Content.ReadAsByteArrayAsync();
                asset = responseData.Deserialize<SimpleAssetDto>();

                await _cache.SetHashByKeyAsync(hashKey, hashField, asset);
            }

            if (asset == null)
            {
                throw new EntityNotFoundException(detailCode: MessageConstants.ASSET_NOT_FOUND);
            }

            return asset;
        }
    }
}
