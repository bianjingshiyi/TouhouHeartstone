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
using Game;
using BJSYGameCore;
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
        CardManager _manager;
        [SerializeField]
        Vector2 _scroll;
        [SerializeField]
        string _templatePath;
        [SerializeField]
        string _outputPath;
        Assembly[] _assemblies;
        GenericMenu _menu;
        protected void OnEnable()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _menu = new GenericMenu();
            foreach (var assembly in _assemblies)
            {
                string name = assembly.GetName().Name.Replace('.', '/');
                bool isSelected = _selectedAssemblyNameList.Contains(name);
                _menu.AddItem(new GUIContent(name), isSelected, () =>
                {
                    if (!_selectedAssemblyNameList.Contains(name))
                        _selectedAssemblyNameList.Add(name);
                    else
                        _selectedAssemblyNameList.Remove(name);
                    loadCardList();
                });
            }
            loadCardList();
        }
        List<CardDefine> _cardList;
        protected void OnGUI()
        {
            if (EditorGUILayout.DropdownButton(new GUIContent("选择编译集"), FocusType.Keyboard))
            {
                _menu.DropDown(GUILayoutUtility.GetLastRect());
            }
            _manager = EditorGUILayout.ObjectField("Table", _manager, typeof(CardManager), true) as CardManager;
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(512), GUILayout.Height(512));
            foreach (var card in _cardList.OrderBy(c => c.id))
            {
                if (!card.isObsolete())
                    GUILayout.Label(card.id + " " + card.GetType().Name);
                else
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.normal.textColor = Color.yellow;
                    GUILayout.Label(card.id + " " + card.GetType().Name, style);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("导出Xml", GUILayout.Width(128)))
            {
                exportAsXml(_cardList);
            }
            if (GUILayout.Button("导出Excel", GUILayout.Width(128)))
            {
                exportAsExcel(_cardList);
            }
            if (GUILayout.Button("覆盖Excel", GUILayout.Width(128)))
            {
                overrideToExcel(_cardList);
            }
            EditorGUILayout.EndHorizontal();
        }
        void loadCardList()
        {
            _cardList = new List<CardDefine>();
            foreach (var assembly in _assemblies)
            {
                string name = assembly.GetName().Name.Replace('.', '/');
                bool isSelected = _selectedAssemblyNameList.Contains(name);
                if (isSelected)
                {
                    _cardList.AddRange(CardHelper.getCardDefines(new Assembly[] { assembly }, new ULogger() { blackList = new List<string>() { "Load" } }));
                }
            }
        }
        void exportAsExcel(List<CardDefine> cardList)
        {
            _manager.load();

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
        void overrideToExcel(List<CardDefine> cardList)
        {
            _manager.load();

            if (trySelectFilePath(ref _outputPath, "选择要覆盖的文件", "Cards", "xls"))
            {
                Workbook workbook;
                using (FileStream stream = new FileStream(_outputPath, FileMode.Open))
                {
                    workbook = Workbook.Load(stream);
                    Worksheet worksheet = workbook.Worksheets[0];
                    foreach (var card in cardList)
                    {
                        if (isExcelContainCard(worksheet, card))
                        {
                            Debug.Log("覆盖" + card);
                            saveCardToExcel(card, worksheet);
                        }
                    }
                }
                using (FileStream stream = new FileStream(_outputPath, FileMode.Create))
                {
                    workbook.Save(stream);
                }
            }
            else
                Debug.LogError("不存在覆盖文件" + Path.GetDirectoryName(_outputPath));
        }
        /// <summary>
        /// 尝试选择文件路径并将它保存为字段，下一次选择路径会优先打开上一次选择的文件夹。
        /// </summary>
        /// <param name="pathField">路径字段</param>
        /// <param name="title"></param>
        /// <param name="defaultName"></param>
        /// <param name="extension">不需要带.</param>
        /// <param name="defaultDir">如果不填，那么是Application.streamingAssetsPath</param>
        /// <returns></returns>
        bool trySelectFilePath(ref string pathField, string title, string defaultName, string extension, string defaultDir = null)
        {
            pathField = EditorUtility.SaveFilePanel(title,
                        File.Exists(pathField) ? Path.GetDirectoryName(pathField) : (Directory.Exists(defaultDir) ? defaultDir : Application.streamingAssetsPath), defaultName, extension);
            if (Directory.Exists(Path.GetDirectoryName(pathField)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        bool isExcelContainCard(Worksheet sheet, CardDefine card)
        {
            int idIndex = findColIndex(sheet, "ID");
            int classIndex = findColIndex(sheet, "Class");
            foreach (var pRow in sheet.Cells.Rows)
            {
                if (numberToInt(pRow.Value.GetCell(idIndex).Value) == card.id)
                {
                    return true;
                }
                else if (pRow.Value.GetCell(classIndex).StringValue == card.GetType().Name)
                {
                    return true;
                }
            }
            return false;
        }
        void saveCardToExcel(CardDefine card, Worksheet sheet)
        {
            int idIndex = findColIndex(sheet, "ID");
            int classIndex = findColIndex(sheet, "Class");
            int cardRow = sheet.Cells.Rows.Count;//如果表里没有这张卡，那么就新增
            foreach (var pRow in sheet.Cells.Rows)
            {
                if (numberToInt(pRow.Value.GetCell(idIndex).Value) == card.id)
                {
                    cardRow = pRow.Key;//如果有相同ID的卡，复写
                    sheet.Cells[cardRow, classIndex] = new Cell(card.GetType().Name);
                }
                else if (pRow.Value.GetCell(classIndex).StringValue == card.GetType().Name)
                {
                    cardRow = pRow.Key;//如果有相同类型的卡，复写
                    sheet.Cells[cardRow, idIndex] = new Cell(card.id);
                }
            }

            sheet.Cells[cardRow, idIndex] = new Cell(card.id);
            int typeIndex = findColIndex(sheet, "Type");
            if (card is ServantCardDefine)
                sheet.Cells[cardRow, typeIndex] = new Cell("Servant");
            else if (card is MasterCardDefine)
                sheet.Cells[cardRow, typeIndex] = new Cell("Master");
            else if (card is SkillCardDefine)
                sheet.Cells[cardRow, typeIndex] = new Cell("Skill");
            else if (card is SpellCardDefine)
                sheet.Cells[cardRow, typeIndex] = new Cell("Spell");
            else if (card is ItemCardDefine)
                sheet.Cells[cardRow, typeIndex] = new Cell("Item");
            int costIndex = findColIndex(sheet, "Cost");
            sheet.Cells[cardRow, costIndex] = new Cell(card.getProp<int>("cost"));
            int attackIndex = findColIndex(sheet, "Attack");
            sheet.Cells[cardRow, attackIndex] = new Cell(card.getProp<int>("attack"));
            int lifeIndex = findColIndex(sheet, "Life");
            sheet.Cells[cardRow, lifeIndex] = new Cell(card.getProp<int>("life"));
            int tagsIndex = findColIndex(sheet, "Tags");
            if (card.getProp<string[]>("tags") is string[] tags)
                sheet.Cells[cardRow, tagsIndex] = new Cell(string.Join(",", tags));
            else
                sheet.Cells[cardRow, tagsIndex] = new Cell(null);
            int keywordsIndex = findColIndex(sheet, "Keywords");
            if (card.getProp<string[]>("keywords") is string[] keywords)
                sheet.Cells[cardRow, keywordsIndex] = new Cell(string.Join(",", keywords));
            else
                sheet.Cells[cardRow, keywordsIndex] = new Cell(null);
            if (_manager != null)
            {
                var skin = _manager.getSkin(card.id);
                int imageIndex = findColIndex(sheet, "Image");
                if (skin == null)
                    sheet.Cells[cardRow, imageIndex] = new Cell(sheet.Cells[cardRow, imageIndex].StringValue);
                else
                {
                    if (skin.image != null && !string.IsNullOrEmpty(skin.image.name))
                    {
                        string path = AssetDatabase.GetAssetPath(skin.image);
                        if (path.Contains("Resources/"))
                        {
                            path = path.removeHead("Resources/").removeRear(".");
                            sheet.Cells[cardRow, imageIndex] = new Cell("res:" + path);
                        }
                        else
                            sheet.Cells[cardRow, imageIndex] = new Cell(skin.image.name + ".png");
                    }
                    else
                        sheet.Cells[cardRow, imageIndex] = new Cell(sheet.Cells[cardRow, imageIndex].StringValue);
                }
                int nameIndex = findColIndex(sheet, "Name");
                sheet.Cells[cardRow, nameIndex] = new Cell(skin == null ? (!string.IsNullOrEmpty(sheet.Cells[cardRow, nameIndex].StringValue) ? sheet.Cells[cardRow, nameIndex].StringValue : card.GetType().Name) : skin.name);
                int descIndex = findColIndex(sheet, "Desc");
                sheet.Cells[cardRow, descIndex] = new Cell(skin == null ? sheet.Cells[cardRow, descIndex].StringValue : skin.desc);
            }
        }
        static int numberToInt(object value)
        {
            if (value is double d)
                return (int)d;
            else if (value is int i)
                return i;
            else if (value is float f)
                return (int)f;
            else
                return 0;
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
            if (_manager != null)
            {
                var skin = _manager.getSkin(card.id);
                xml["Card"]["Skin"]["Image"].InnerText = skin == null ? null : skin.image != null ? skin.image.name + ".png" : null;
                xml["Card"]["Skin"]["Name"].InnerText = skin == null ? card.GetType().Name : skin.name;
                xml["Card"]["Skin"]["Desc"].InnerText = skin == null ? null : skin.desc;
            }
        }
    }
}