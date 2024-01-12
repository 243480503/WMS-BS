/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Code
{
    public class ExcelHelper
    {

        /// <summary>
        /// 读取Excel导入DataTable
        /// </summary>
        /// <param name="filepath">导入的文件路径（包括文件名）</   param>
        /// <param name="sheetName">工作表名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列    名</param>
        /// <returns>DataTable</returns>
        public static DataTable ExcelToDataTable(string filePath, string sheetName, bool isFirstRowColumn)
        {
            DataTable data = new DataTable();
            FileStream fs;
            int startRow = 0;
            using (fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    IWorkbook workbook = null; // filePath.Contains(".xlsx") ? (IWorkbook)new XSSFWorkbook(fs) : new HSSFWorkbook(fs);//xlsx使用XSSFWorkbook， xls使用HSSFWorkbokk
                    try
                    {
                        workbook = (IWorkbook)new XSSFWorkbook(fs);
                    }
                    catch(Exception e)
                    {
                        workbook = (IWorkbook)new HSSFWorkbook(fs);
                    }
                    ISheet sheet = workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);//如果没有找到指sheetName 对应的sheet，则尝试获取第一个sheet
                    if (sheet != null)
                    {
                        IRow firstrow = sheet.GetRow(0);//第一行
                        int firstCellNum = firstrow.FirstCellNum;// 行第一个cell的编号,从0开始
                        int lastCellNum = firstrow.LastCellNum; //  行最后一个cell的编号 即总的列数,（不忽略中间某    列空格）
                        if (isFirstRowColumn)//如果第一行是表格列头
                        {
                            for (int i = firstCellNum; i < lastCellNum; i++)
                            {
                                ICell cell = firstrow.GetCell(i);
                                if (cell != null)
                                {
                                    string cellValue = cell.StringCellValue;
                                    if (cellValue != null)
                                    {
                                        DataColumn column = new DataColumn(cellValue);
                                        data.Columns.Add(column);
                                    }
                                }
                            }
                            startRow = sheet.FirstRowNum + 1;
                        }
                        else
                        {
                            startRow = sheet.FirstRowNum;
                        }
                        //读数据行
                        int rowCont = sheet.LastRowNum;
                        for (int i = startRow; i <= rowCont; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            DataRow dataRow = data.NewRow();
                            //判断需要读取的最后一行
                            if (row != null && (row.GetCell(row.FirstCellNum) != null && row.GetCell(row.FirstCellNum).ToString() != "合计"))
                            {
                                for (int j = row.FirstCellNum; j < lastCellNum; j++)
                                {
                                    dataRow[j] = row.GetCell(j)==null?"":row.GetCell(j).ToString();
                                }
                                data.Rows.Add(dataRow);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    return data;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
    }
}
