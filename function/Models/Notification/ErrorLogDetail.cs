using System.Collections.Generic;
using System.Linq;
using AHI.Infrastructure.SharedKernel.Model;

namespace AHI.AssetTable.Function.Model.Notification
{
    public class ErrorLogDetail
    {
        public IEnumerable<TrackError> Errors { get; set; }
        public ErrorLogDetail(IEnumerable<TrackError> errors)
        {
            if (errors != null && errors.Any())
            {
                Errors = errors;
            }
        }
    }
}
