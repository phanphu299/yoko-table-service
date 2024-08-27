using System.Collections.Generic;
namespace Function.Model.ImportModel
{

    public class AssetTableModel : Dictionary<string, object>
    {
        public int Row { get; set; }
        public string RawValue { get; set; }
    }
}
