using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using UnityEngine;
using UnityEditor;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using UI;
using ExcelLibrary;
using ExcelLibrary.SpreadSheet;
namespace TouhouHeartstone
{
    class CardDefineExporterWindow : EditorWindow
    {
        [MenuItem("Window/TouhouHeartstone/CardDefineExporter")]
        public static void open()
        {
            GetWindow<CardDefineExporterWindow>("CardDefineExporter");
        }
        [SerializeField]
        List<string> _selectedAssemblyNameList = new List<string>();
        [SerializeField]
        Table _table;
        [SerializeField]
        Vector2 _scroll;
        [SerializeField]
        string _templatePath;
        [SerializeField]
        string _outputPath;
        private void OnGUI()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            GenericMenu menu = new GenericMenu();
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (var assembly in assemblies)
            {
                string name = assembly.GetName().Name.Replace('.', '/');
                bool isSelected = _selectedAssemblyNameList.Contains(name);
                if (isSelected)
                {
                    cardList.AddRange(CardHelper.getCardDefines(assembly));
                }
                menu.AddItem(new GUIContent(name), isSelected, () =>
                {
                    if (!_selectedAssemblyNameList.Contains(name))
                        _selectedAssemblyNameList.Add(name);
                    else
                        _selectedAssemblyNameList.Remove(name);
                });
            }
            if (EditorGUILayout.DropdownButton(new GUIContent("选择编译集"), FocusType.Keyboard))
            {
                menu.DropDown(GUILayoutUtility.GetLastRect());
            }
            _table = EditorGUILayout.ObjectField("Table", _table, typeof(Table), true) as Table;
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(512), GUILayout.Height(512));
            foreach (var card in cardList)
            {
                GUILayout.Label(card.GetType().Name);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("导出Xml", GUILayout.Width(128)))
            {
                exportAsXml(cardList);
            }
            if (GUILayout.Button("导出Excel", GUILayout.Width(128)))
            {
                exportAsExcel(cardList);
            }
            EditorGUILayout.EndHorizontal();
        }
        private void exportAsExcel(List<CardDefine> cardList)
        {
            _templatePath = EditorUtility.OpenFilePanel("选择模板文件",
                File.Exists(_templatePath) ? Path.GetDirectoryName(_templatePath) : Application.dataPath, "xls");
            if (File.Exists(_templatePath))
            {
                using (FileStream stream = new FileStream(_templatePath, FileMode.Open))
                {
                    Workbook workbook = Workbook.Load(stream);
                    Worksheet worksheet = workbook.Worksheets[0];
                    _outputPath = EditorUtility.SaveFilePanel("选择输出路径",
                        File.Exists(_outputPath) ? Path.GetDirectoryName(_outputPath) : Application.streamingAssetsPath, "Cards", "xls");
                    if (Directory.Exists(Path.GetDirectoryName(_outputPath)))
                    {
                        using (FileStream outputStream = new FileStream(_outputPath, FileMode.Create))
                        {
                            foreach (var card in cardList)
                            {
                                saveCardToExcel(card, worksheet);
                            }
                            workbook.Save(outputStream);
                        }
                    }
                    else
                        Debug.LogError("不存在输出文件夹" + Path.GetDirectoryName(_outputPath));
                }
            }
            else
                Debug.LogError("不存在模板文件" + _templatePath);
        }
        void saveCardToExcel(CardDefine card, Worksheet sheet)
        {
            int row = sheet.Cells.Rows.Count;
            int idIndex = findColIndex(sheet, "ID");
            sheet.Cells[row, idIndex] = new Cell(card.id);
            int typeIndex = findColIndex(sheet, "Type");
            if (card is ServantCardDefine)
                sheet.Cells[row, typeIndex] = new Cell("Servant");
            else if (card is MasterCardDefine)
                sheet.Cells[row, typeIndex] = new Cell("Master");
            else if (card is SkillCardDefine)
                sheet.Cells[row, typeIndex] = new Cell("Skill");
            else if (card is SpellCardDefine)
                sheet.Cells[row, typeIndex] = new Cell("Spell");
            int costIndex = findColIndex(sheet, "Cost");
            sheet.Cells[row, costIndex] = new Cell(card.getProp<int>("cost"));
            int attackIndex = findColIndex(sheet, "Attack");
            sheet.Cells[row, attackIndex] = new Cell(card.getProp<int>("attack"));
            int lifeIndex = findColIndex(sheet, "Life");
            sheet.Cells[row, lifeIndex] = new Cell(card.getProp<int>("life"));
            int tagsIndex = findColIndex(sheet, "Tags");
            if (card.getProp<string[]>("tags") is string[] tags)
                sheet.Cells[row, tagsIndex] = new Cell(string.Join(",", tags));
            else
                sheet.Cells[row, tagsIndex] = new Cell(null);
            int keywordsIndex = findColIndex(sheet, "Keywords");
            if (card.getProp<string[]>("keywords") is string[] keywords)
                sheet.Cells[row, keywordsIndex] = new Cell(string.Join(",", keywords));
            else
                sheet.Cells[row, keywordsIndex] = new Cell(null);
            if (_table != null)
            {
                var skin = _table.getSkin(card);
                int imageIndex = findColIndex(sheet, "Image");
                sheet.Cells[row, imageIndex] = new Cell(skin == null ? null : skin.image != null ? skin.image.name + ".png" : null);
                int nameIndex = findColIndex(sheet, "Name");
                sheet.Cells[row, nameIndex] = new Cell(skin == null ? card.GetType().Name : skin.cardName);
                int descIndex = findColIndex(sheet, "Desc");
                sheet.Cells[row, descIndex] = new Cell(skin == null ? null : skin.desc);
            }
        }
        int findColIndex(Worksheet worksheet, string name)
        {
            foreach (var p in worksheet.Cells.Rows[0])
            {
                if (p.Value.StringValue == name)
                    return p.Key;
            }
            return -1;
        }
        private void exportAsXml(List<CardDefine> cardList)
        {
            _templatePath = EditorUtility.OpenFilePanel("选择模板文件",
                File.Exists(_templatePath) ? Path.GetDirectoryName(_templatePath) : Application.dataPath, "xml");
            if (File.Exists(_templatePath))
            {
                using (FileStream stream = new FileStream(_templatePath, FileMode.Open))
                {
                    XmlDocument templateXml = new XmlDocument();
                    templateXml.Load(stream);
                    _outputPath = EditorUtility.SaveFolderPanel("选择输出路径",
                        Directory.Exists(_outputPath) ? _outputPath : Application.streamingAssetsPath, "");
                    if (Directory.Exists(_outputPath))
                    {
                        foreach (var card in cardList)
                        {
                            using (StreamWriter writer = new StreamWriter(_outputPath + "/" + card.GetType().Name + ".xml"))
                            {
                                saveCardToXml(card, templateXml);
                                templateXml.Save(writer);
                            }
                        }
                    }
                    else
                        Debug.LogError("不存在输出文件夹" + _outputPath);
                }
            }
            else
                Debug.LogError("不存在模板文件" + _templatePath);
        }

        void saveCardToXml(CardDefine card, XmlDocument xml)
        {
            xml["Card"]["ID"].InnerText = card.id.ToString();
            if (card is ServantCardDefine)
                xml["Card"]["Type"].InnerText = "Servant";
            else if (card is MasterCardDefine)
                xml["Card"]["Type"].InnerText = "Master";
            else if (card is SkillCardDefine)
                xml["Card"]["Type"].InnerText = "Skill";
            xml["Card"]["Cost"].InnerText = card.getProp<int>("cost").ToString();
            xml["Card"]["Attack"].InnerText = card.getProp<int>("attack").ToString();
            xml["Card"]["Life"].InnerText = card.getProp<int>("life").ToString();
            if (card.getProp<string[]>("tags") is string[] tags)
                xml["Card"]["Tags"].InnerText = string.Join(",", tags);
            else
                xml["Card"]["Tags"].InnerText = null;
            if (card.getProp<string[]>("keywords") is string[] keywords)
                xml["Card"]["Keywords"].InnerText = string.Join(",", keywords);
            else
                xml["Card"]["Keywords"].InnerText = null;
            if (_table != null)
            {
                var skin = _table.getSkin(card);
                xml["Card"]["Skin"]["Image"].InnerText = skin == null ? null : skin.image != null ? skin.image.name + ".png" : null;
                xml["Card"]["Skin"]["Name"].InnerText = skin == null ? card.GetType().Name : skin.cardName;
                xml["Card"]["Skin"]["Desc"].InnerText = skin == null ? null : skin.desc;
            }
        }
    }
}