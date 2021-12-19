using System;
using ExcelLibrary.SpreadSheet;
namespace TouhouHeartstone
{
    public static class ExcelHelper
    {
        public static int findColIndex(Worksheet worksheet, string name)
        {
            foreach (var p in worksheet.Cells.Rows[0])
            {
                if (p.Value.StringValue == name)
                    return p.Key;
            }
            return -1;
        }
        public static int addCol(Worksheet worksheet, string name)
        {
            Row headRow = worksheet.Cells.GetRow(0);
            for (int i = 0; i < headRow.FirstColIndex; i++)
            {
                Cell cell = headRow.GetCell(i);
                if (cell == null)
                    continue;
                if (string.IsNullOrEmpty(cell.StringValue))
                {
                    cell.Value = name;
                    return i;
                }
            }
            for (int i = headRow.FirstColIndex; i < headRow.LastColIndex; i++)
            {
                Cell cell = headRow.GetCell(i);
                if (string.IsNullOrEmpty(cell.StringValue))
                {
                    cell.Value = name;
                    return i;
                }
            }
            headRow.SetCell(headRow.LastColIndex + 1, new Cell(name));
            return headRow.LastColIndex;
        }
        /// <summary>
        /// 将sheetA中指定列的内容（如果不存在则忽略）复制到sheetB中的同名列中（如果不存在则新增）然后将其删除。
        /// sheetA的单元格所在行的ID必须与sheetB中单元格所在行的ID一致，如果sheetB中不存在对应ID行，则新增一行。
        /// </summary>
        /// <param name="sheetA"></param>
        /// <param name="sheetB"></param>
        /// <param name="cols"></param>
        /// <remarks>
        /// sheet的第0行为head，其他行可能是别的乱七八糟的注释和元数据，但是剩下的行都是数据行。
        /// sheet一定有一个ID列，其他列的位置不确定，ID列内容为非0数字的是数据行。
        /// </remarks>
        public static void migrateCols(Worksheet sheetA, Worksheet sheetB, string[] cols)
        {
            throw new NotImplementedException();
        }
    }
}