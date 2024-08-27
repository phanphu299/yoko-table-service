using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;

namespace AHI.AssetTable.Function.Model.Notification
{
    public class ExportDetail : ErrorLogDetail
    {
        public string URL { get; set; }
        public ExportDetail(string url, ICollection<TrackError> errors) : base(errors)
        {
            URL = url;
        }
    }
}
