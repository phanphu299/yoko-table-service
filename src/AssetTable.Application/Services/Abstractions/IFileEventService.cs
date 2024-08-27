using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetTable.Application.Service.Abstraction
{
    public interface IFileEventService
    {
        Task SendImportEventAsync(Guid tableId, string tableName, IEnumerable<string> data);
        Task SendExportEventAsync(Guid activityId, Guid tableId, string tableName, string filter);
    }
}
