using System;

namespace AssetTable.Application.Command.Model
{
    public class EntityLockDto
    {
        public Guid TargetId { get; set; }
        public string CurrentUserUpn { get; set; }
        public string SourceId { get; set; }
        public string SourceType { get; set; }
    }
}
