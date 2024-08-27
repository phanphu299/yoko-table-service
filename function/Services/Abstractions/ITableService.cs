using System;
using System.Threading.Tasks;
namespace AHI.AssetTable.Function.Service.Abstraction
{
    public interface ITableService
    {
        Task UpdateAssetAsync(Guid assetId, string assetName, string resourcePath);
        Task DeleteTableAsync(Guid assetId, bool deleteTable);
    }
}
