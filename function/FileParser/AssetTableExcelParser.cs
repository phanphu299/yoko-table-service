using System.Collections.Generic;
using System.Linq;
using AHI.Infrastructure.Import.Extension;
using AHI.Infrastructure.Import.Handler;
using Function.Model.ImportModel;
using NPOI.SS.UserModel;

namespace Function.FileParser
{
    internal class AssetTableExcelParser : ExcelFileHandler<AssetTableModel>
    {
        protected override IEnumerable<AssetTableModel> Parse(ISheet reader)
        {
            var headerNames = new List<string>();
            var headerRow = reader.GetRow(3);
            for (int i = 0; i < headerRow.Cells.Count; i++)
            {
                var cell = headerRow.Cells[i];
                if (string.IsNullOrEmpty(cell.StringCellValue))
                    break;

                headerNames.Add(cell.StringCellValue);
            }
            var records = new List<AssetTableModel>();
            var endRow = reader.LastRowNum;
            var currentRow = 4;
            while (currentRow <= endRow)
            {
                var record = new AssetTableModel();
                // for excel, the row index is less than current ROW in the UI
                record.Row = currentRow + 1;
                var row = reader.GetRow(currentRow);
                if (row != null)
                {
                    for (int i = 0; i < headerNames.Count; i++)
                    {
                        var cell = row.GetCell(i);
                        record[headerNames[i]] = cell?.GetFormatedValue();
                    }
                    // skip the record which has all null value
                    if (record.Any(x => x.Value != null))
                    {
                        records.Add(record);
                    }
                }
                currentRow++;
            }

            return records;
        }
    }
}
