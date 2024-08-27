using System;
using System.Threading.Tasks;

namespace AHI.AssetTable.Function.Service.Abstraction
{
    public interface IAssetTableExportHandler
    {
        Task<string> HandleAsync(string workingDirectory, Guid tableId, string filter);
    }
}
