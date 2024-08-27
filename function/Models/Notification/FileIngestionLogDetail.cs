using System.Collections.Generic;
using System.IO;
using AHI.Infrastructure.SharedKernel.Model;

namespace AHI.AssetTable.Function.Model.Notification
{
    public class FileIngestionLogDetail : ErrorLogDetail
    {
        public string File { get; set; }
        public string URL { get; set; }
        public FileIngestionLogDetail(string filePath, ICollection<TrackError> errors) : base(errors)
        {
            File = Path.GetFileName(filePath);
            URL = filePath;
        }
    }
}
