using System.Linq;
using System.IO;

using UnityEngine;
using UnityEditor;
using ExcelLibrary.SpreadSheet;
using BJSYGameCore;
using MukioI18n;

namespace TouhouHeartstone
{
    class CardSkinExporter
    {
        /// <summary>
        /// 先把卡牌页的遗留皮肤信息转移到皮肤页中，再将皮肤页的信息导出并保存表格。
        /// </summary>
        [MenuItem("Custom/Translate/导出卡牌皮肤翻译")]
        public static void exportSkinTranslation()
        {
            if (!EditorHelper.trySelectFilePath(ref _outputPath, "选择要覆盖的文件", "Cards", "xls", isOpenFile: true))
            {
                Debug.LogError("不存在文件" + _outputPath);
                return;
            }
            Workbook workbook;
            using (FileStream stream = new FileStream(_outputPath, FileMode.Open))
            {
                workbook = Workbook.Load(stream);
            }
            Worksheet cardsheet = workbook.Worksheets.FirstOrDefault(b => b.Name == "Cards");
            if (cardsheet == null)
            {
                Debug.LogError("不存在Cards Sheet");
                return;
            }
            int cardIdIndex = ExcelHelper.findColIndex(cardsheet, "ID");
            if (cardIdIndex < 0)
            {
                Debug.LogError("Cards Sheet 不存在ID 列");
                return;
            }
            //定位卡牌页的属性列
            int cardNameIndex = ExcelHelper.findColIndex(cardsheet, "Name");
            int cardImageIndex = ExcelHelper.findColIndex(cardsheet, "Image");
            int cardIllustratorIndex = ExcelHelper.findColIndex(cardsheet, "Illustrator");
            int cardDescIndex = ExcelHelper.findColIndex(cardsheet, "Desc");
            int cardIgnoreIndex = ExcelHelper.findColIndex(cardsheet, "Ignore");
            //皮肤表的迁移暂时鸽了。
            //Worksheet skinsheet = workbook.Worksheets.FirstOrDefault(b => b.Name == "Skins");
            //int skinIdIndex;
            //int skinNameIndex;
            //int skinImageIndex;
            //int skinIllustratorIndex;
            //int skinDescIndex;
            //if (skinsheet == null)
            //{
            //    //没有皮肤表，新建
            //    skinsheet = new Worksheet("Skins");
            //    skinIdIndex = 0;
            //    skinNameIndex = 2;
            //    skinImageIndex = 4;
            //    skinIllustratorIndex = 5;
            //    skinDescIndex = 6;
            //    skinsheet.Cells[0, 0] = new Cell("ID");
            //    skinsheet.Cells[0, 2] = new Cell("Name");
            //    skinsheet.Cells[0, 4] = new Cell("Image");
            //    skinsheet.Cells[0, 5] = new Cell("Illustrator");
            //    skinsheet.Cells[0, 6] = new Cell("Desc");
            //    workbook.Worksheets.Add(skinsheet);
            //}
            //else
            //{
            //    //有皮肤表，如果没有对应列则添加
            //    skinIdIndex = ExcelHelper.findColIndex(skinsheet, "ID");
            //    if (skinIdIndex < 0)
            //    {
            //        Debug.LogError("Skins Sheet 不存在ID 列");
            //        return;
            //    }
            //    skinNameIndex = ExcelHelper.findColIndex(skinsheet, "Name");
            //    if (skinNameIndex < 0)
            //        skinNameIndex = ExcelHelper.addCol(skinsheet, "Name");
            //    skinImageIndex = ExcelHelper.findColIndex(skinsheet, "Image");
            //    if (skinImageIndex < 0)
            //        skinImageIndex = ExcelHelper.addCol(skinsheet, "Image");
            //    skinIllustratorIndex = ExcelHelper.findColIndex(skinsheet, "Illustrator");
            //    if (skinIllustratorIndex < 0)
            //        skinIllustratorIndex = ExcelHelper.addCol(skinsheet, "Illustrator");
            //    skinDescIndex = ExcelHelper.findColIndex(skinsheet, "Desc");
            //    if (skinDescIndex < 0)
            //        skinDescIndex = ExcelHelper.addCol(skinsheet, "Desc");
            //}
            ////把遗留信息从卡牌表转移到皮肤表
            //foreach (var cardRow in cardsheet.Cells.Rows.Values)
            //{
            //    if (int.TryParse(cardRow.GetCell(cardIdIndex).StringValue, out int cardId))
            //    {
            //        int skinCardId = -1;
            //        //查找是否已存在皮肤
            //        foreach (var skinRow in skinsheet.Cells.Rows.Values)
            //        {
            //            if (int.TryParse(skinRow.GetCell(skinIdIndex).StringValue, out skinCardId) && skinCardId == cardId)
            //            {
            //                //发现已有皮肤信息，覆盖
            //                if (cardNameIndex >= 0)
            //                    skinRow.GetCell(skinNameIndex).Value = cardRow.GetCell(cardNameIndex).Value;
            //                if (cardImageIndex >= 0)
            //                    skinRow.GetCell(skinImageIndex).Value = cardRow.GetCell(cardImageIndex).Value;
            //                if (cardIllustratorIndex >= 0)
            //                    skinRow.GetCell(skinIllustratorIndex).Value = cardRow.GetCell(cardIllustratorIndex).Value;
            //                if (cardDescIndex >= 0)
            //                    skinRow.GetCell(skinDescIndex).Value = cardRow.GetCell(cardDescIndex).Value;
            //                break;
            //            }
            //        }
            //        if (skinCardId < 0)
            //        {
            //            //没有已存在皮肤，新增
            //            Row skinRow = new Row();
            //            skinRow.SetCell(skinIdIndex, new Cell(cardId));
            //            skinRow.SetCell(1, new Cell(0));
            //            if (cardNameIndex >= 0)
            //                skinRow.SetCell(skinNameIndex, new Cell(cardRow.GetCell(cardNameIndex).Value));
            //            if (cardImageIndex >= 0)
            //                skinRow.SetCell(skinImageIndex, new Cell(cardRow.GetCell(cardImageIndex).Value));
            //            if (cardIllustratorIndex >= 0)
            //                skinRow.SetCell(skinIllustratorIndex, new Cell(cardRow.GetCell(cardIllustratorIndex).Value));
            //            if (cardDescIndex >= 0)
            //                skinRow.SetCell(skinDescIndex, new Cell(cardRow.GetCell(cardDescIndex).Value));
            //            skinsheet.Cells.Rows.Add(skinsheet.Cells.LastRowIndex + 1, skinRow);
            //        }
            //    }
            //}
            ////删除老的列
            //foreach (var row in cardsheet.Cells.Rows.Values)
            //{
            //    if (cardDescIndex>=0)
            //}
            MukioPotGenerator potGenerator = new MukioPotGenerator("ZMCS", "Translation Team");
            foreach (var pair in cardsheet.Cells.Rows)
            {
                if (pair.Key == 0 || pair.Key == 1)
                    continue;
                Row row = pair.Value;
                if (row.GetCell(cardIgnoreIndex).Value is bool ignore && ignore)
                    continue;
                string name = row.GetCell(cardNameIndex).StringValue;
                //string image = row.GetCell(cardImageIndex).StringValue;
                //string illu = row.GetCell(cardIllustratorIndex).StringValue;
                string desc = row.GetCell(cardDescIndex).StringValue;
                potGenerator.AddString(name);
                potGenerator.AddString(desc);
            }

            string path = Path.Combine(FileLoader.FolderPath, "i18n", "skin", Path.GetFileNameWithoutExtension(_outputPath) + ".pot");
            potGenerator.WriteOut(path);
            Debug.Log("导出皮肤文件" + path + "成功");
        }
        private static string _outputPath;
    }
}