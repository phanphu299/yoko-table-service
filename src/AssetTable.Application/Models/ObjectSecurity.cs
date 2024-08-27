using System.Collections.Generic;

namespace AssetTable.Application.Model
{
    public class ObjectSecurity
    {
        public ObjectSecurity() { }
        public ObjectSecurity(bool fullAccess, IEnumerable<string> restrictedIds, IEnumerable<string> allowedIds)
        {
            FullAccess = fullAccess;
            RestrictedIds = restrictedIds;
            AllowedIds = allowedIds;
        }
        public bool FullAccess { get; set; }
        public IEnumerable<string> RestrictedIds { get; set; }
        public IEnumerable<string> AllowedIds { get; set; }
        public string Upn { get; set; }
    }

}
