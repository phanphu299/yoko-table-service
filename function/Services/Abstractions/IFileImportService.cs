using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AHI.Infrastructure.Audit.Model;

namespace Function.Service.Abstraction
{
    public interface IFileImportService
    {
        Task<ImportExportBasePayload> ImportFileAsync(string upn
        , Guid activityId, string workingDirectory, Guid tableId, IEnumerable<string> fileNames, string dateTimeFormat,
           AHI.Infrastructure.UserContext.Models.TimeZone timeZone);
    }
}
