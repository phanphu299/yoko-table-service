using System;
using System.Threading;
using System.Threading.Tasks;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.Service.Abstraction
{
    public interface IAssetService
    {
        Task<SimpleAssetDto> FetchAsync(Guid assetId, CancellationToken token);
    }
}
