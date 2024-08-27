using System;
using AHI.Infrastructure.SharedKernel.Model;

namespace AssetTable.Application.Models
{
    public class ActivityResponse : BaseResponse
    {
        public Guid ActivityId { get; set; }

        public ActivityResponse(Guid activityId)
            : base(true, null)
        {
            ActivityId = activityId;
        }
    }
}
