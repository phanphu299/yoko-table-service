using System;
using System.Collections.Generic;

namespace AssetTable.Application.Model
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Upn { get; set; }
        public string UserTypeCode { get; set; }
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";
        public string DisplayDateTimeFormat { get; set; } = "YYYY-MM-DD HH:mm:ss.SSS";
        public bool IsLocked { get; set; }
        public string Avatar { get; set; }
        public IEnumerable<Guid> Groups { get; set; }
        public IEnumerable<string> RightHashes { get; set; }
        public IEnumerable<string> ObjectRightShorts { get; set; }
        public IEnumerable<string> ObjectRightHashes { get; set; }
        public IEnumerable<string> Rights { get; set; }
        public IEnumerable<string> RightShorts { get; set; }
        public IEnumerable<string> ObjectRights { get; set; }
        public string AppEndpoint { get; set; }
        public string HomeTenantId { get; set; }
        public string HomeSubscriptionId { get; set; }
        public bool IsLocalUser { get; set; }
    }
}
