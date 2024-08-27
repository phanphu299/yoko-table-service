using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using AHI.Infrastructure.Import.Abstraction;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using static AHI.AssetTable.Function.Service.AssetTableExportHandler;
using AHI.AssetTable.Function.Constant;

namespace Function.FileParser
{
    public class AssetTableExcelExport
    {
        private IWorkbook _workbook;

        private ISheet _sheet;

        private readonly ICollection<string> _properties;
        private readonly IParserContext _context;

        public AssetTableExcelExport(IParserContext context)
        {
            _workbook = new XSSFWorkbook();
            _properties = new List<string>();
            _context = context;
        }

        public void SetTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                throw new ArgumentException("templateName is required");
            }

            DataTable dataTable = new DataTable();
            List<string> list = new List<string>();
            using FileStream fileStream = new FileStream(templateName, FileMode.Open);
            fileStream.Position = 0L;
            _workbook = new XSSFWorkbook((Stream)fileStream);
            ISheet sheetAt = _workbook.GetSheetAt(0);
            IRow row = sheetAt.GetRow(0);
            IRow row3 = sheetAt.GetRow(3);
            int lastCellNum = row.LastCellNum;
            for (int i = 0; i < lastCellNum; i++)
            {
                ICell cell = row3.GetCell(i);
                if (cell != null)
                {
                    string item = cell.ToString()!.Replace("<", "").Replace(">", "");
                    _properties.Add(item);
                }
            }
        }
        public void SetData(string sheetName, string tableName, Guid tableId, IEnumerable<ColumnPostgres> tableColumn, IEnumerable<dynamic> data)
        {
            DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data), typeof(DataTable));
            ISheet sheetAt = _workbook.GetSheetAt(0);
            _sheet = sheetAt.CopySheet(sheetName, copyStyle: true);
            IList<string> list = new List<string>();
            IRow row = _sheet.CreateRow(0);
            IRow row1 = _sheet.CreateRow(1);
            IRow row3 = _sheet.CreateRow(3);
            int num = 0;
            row.CreateCell(num).SetCellValue("Table: " + tableName);
            row1.CreateCell(num).SetCellValue("Table Id: " + tableId);
            foreach (var column in tableColumn)
            {
                row3.CreateCell(num).SetCellValue(column.Name);
                num++;
            }
            int rowData = 4;
            var offset = TimeSpan.Parse(_context.TimezoneOffset);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                row = _sheet.CreateRow(rowData);
                int columnData = 0;
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    var value = dataRow[columnData];
                    if (tableColumn.ElementAt(columnData).DataType.ToLower() == DataTypeConstants.TYPE_DATETIME && value != DBNull.Value)
                    {
                        var datetime = Convert.ToDateTime(value);
                        var datetimeOffsetValue = new DateTimeOffset(datetime, TimeSpan.Zero).ToOffset(offset);
                        value = datetimeOffsetValue.ToString(_context.DateTimeFormat);
                    }
                    row.CreateCell(columnData).SetCellValue(value?.ToString());
                    columnData++;
                }
                rowData++;
            }
        }
        public byte[] BuildExcelStream()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                _workbook.RemoveSheetAt(0);
                _workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
