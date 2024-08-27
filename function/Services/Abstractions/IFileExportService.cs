using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AHI.Infrastructure.Audit.Model;

namespace AHI.AssetTable.Function.Service.Abstraction
{
    public interface IFileExportService
    {
        Task<ImportExportBasePayload> ExportFileAsync(
            Guid tableId,
            string upn,
            Guid activityId,
            ExecutionContext context,
            string fitlter,
            string dateTimeFormat,
           Infrastructure.UserContext.Models.TimeZone timeZone
        );
    }
}
