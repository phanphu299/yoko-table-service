using System;
using System.IO;
using System.Threading.Tasks;

namespace Function.Service.Abstraction
{
    public interface IAssetTableService
    {
        Task RunImportAsync(Guid tableId, Stream stream);
    }
}
