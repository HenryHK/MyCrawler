namespace MyCrawler
{
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using System;
    using System.Data;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class ExcelHelperNew
    {
        public static string DataTableToExcel(DataTable data, string sheetName, string excelSavePath, bool isColumnWritten = true)
        {
            int num = 0;
            int columnIndex = 0;
            ISheet sheet = null;
            IWorkbook workbook = null;
            try
            {
                if (excelSavePath.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook();
                }
                else if (excelSavePath.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook();
                }
                if (string.IsNullOrEmpty(sheetName))
                {
                    sheetName = "sheet1";
                }
                sheet = workbook.CreateSheet(sheetName);
                IRow row = null;
                if (isColumnWritten)
                {
                    row = sheet.CreateRow(0);
                    columnIndex = 0;
                    while (columnIndex < data.Columns.Count)
                    {
                        sheet.SetColumnWidth(columnIndex, 0x1400);
                        row.CreateCell(columnIndex).SetCellValue(data.Columns[columnIndex].ColumnName);
                        columnIndex++;
                    }
                }
                for (num = 0; num < data.Rows.Count; num++)
                {
                    row = sheet.CreateRow(num + 1);
                    for (columnIndex = 0; columnIndex < data.Columns.Count; columnIndex++)
                    {
                        switch (data.Columns[columnIndex].DataType.ToString())
                        {
                            case "System.String":
                            {
                                string str2 = data.Rows[num][columnIndex].ToString();
                                row.CreateCell(columnIndex).SetCellValue(data.Rows[num][columnIndex].ToString());
                                break;
                            }
                            case "System.DateTime":
                            {
                                string str3 = data.Rows[num][columnIndex].ToString();
                                if (!string.IsNullOrEmpty(str3))
                                {
                                    str3 = DateTime.Parse(str3).ToString("yyyy-MM-dd hh:mm:ss");
                                }
                                row.CreateCell(columnIndex).SetCellValue(str3);
                                break;
                            }
                            case "System.Boolean":
                            {
                                bool result = false;
                                bool.TryParse(data.Rows[num][columnIndex].ToString(), out result);
                                row.CreateCell(columnIndex).SetCellValue(result);
                                break;
                            }
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                            case "System.Byte":
                            {
                                int num3 = 0;
                                int.TryParse(data.Rows[num][columnIndex].ToString(), out num3);
                                row.CreateCell(columnIndex).SetCellValue((double) num3);
                                break;
                            }
                            case "System.Decimal":
                            case "System.Double":
                            {
                                double num4 = 0.0;
                                double.TryParse(data.Rows[num][columnIndex].ToString(), out num4);
                                row.CreateCell(columnIndex).SetCellValue(num4);
                                break;
                            }
                            case "System.DBNull":
                                row.CreateCell(columnIndex).SetCellValue("");
                                break;

                            default:
                                row.CreateCell(columnIndex).SetCellValue("");
                                break;
                        }
                    }
                }
                using (FileStream stream = new FileStream(excelSavePath, FileMode.Create))
                {
                    workbook.Write(stream);
                }
                return "sucess";
            }
            catch (Exception exception)
            {
                return ("error|" + exception.Message);
            }
        }

        public string ExcelModelPath { get; set; }

        public string ExcelSavePath { get; set; }
    }
}

