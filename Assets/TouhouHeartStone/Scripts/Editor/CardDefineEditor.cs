using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TouhouHeartstone.Backend
{
    class CardDefineEditorWindow : EditorWindow
    {
        [MenuItem("Window/TouhouHeartstone/CardDefineEditor")]
        public static void open()
        {
            GetWindow<CardDefineEditorWindow>("CardDefine");
        }
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            //绘制文件夹与文件
            drawAllFiles();
            GUILayout.BeginVertical();
            //显示文件选项，保存与另存为
            drawFileOptions();
            //显示CardDefine对象编辑界面
            drawCardEditor();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        void drawAllFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
            if (!dir.Exists)
                dir.Create();
            _filePanelScrollPosition = GUILayout.BeginScrollView(_filePanelScrollPosition, GUILayout.Width(150));
            drawDir(dir);
            GUILayout.EndScrollView();
        }
        Vector2 _filePanelScrollPosition;
        void drawDir(DirectoryInfo dir)
        {
            int dirHash = dir.FullName.GetHashCode();
            if (!dicDirIsOpen.ContainsKey(dirHash))
                dicDirIsOpen.Add(dirHash, false);
            GUILayout.BeginVertical();
            dicDirIsOpen[dirHash] = EditorGUILayout.Foldout(dicDirIsOpen[dirHash], dir.Name, true);
            if (dicDirIsOpen[dirHash])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                foreach (DirectoryInfo childDir in dir.GetDirectories())
                {
                    drawDir(childDir);
                }
                foreach (FileInfo file in dir.GetFiles("*.thcd"))
                {
                    if (GUILayout.Button(file.Name.Substring(0, file.Name.Length - 5), GUI.skin.label))
                    {
                        _currentPath = file.FullName;
                        _card = CardFileHelper.readFromFile(_currentPath);
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        Dictionary<int, bool> dicDirIsOpen { get; } = new Dictionary<int, bool>();
        void drawFileOptions()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("新建"))
            {
                _card = new GeneratedCardDefine();
            }
            if (GUILayout.Button("保存"))
            {
                saveFile();
            }
            if (GUILayout.Button("另存为"))
            {
                saveAsFile();
            }
            GUILayout.EndHorizontal();
        }
        private void saveFile()
        {
            if (!string.IsNullOrEmpty(_currentPath))
                CardFileHelper.writeToFile(_currentPath, _card);
            else
                saveAsFile();
        }
        private void saveAsFile()
        {
            _currentPath = EditorUtility.SaveFilePanel(string.Empty, string.IsNullOrEmpty(_currentPath) ? Application.streamingAssetsPath : _currentPath, "New Card", "thcd");
            if (!string.IsNullOrEmpty(_currentPath))
                CardFileHelper.writeToFile(_currentPath, _card);
        }
        string _currentPath = string.Empty;
        void drawCardEditor()
        {
            if (_card != null)
            {
                _card.setProp("id", EditorGUILayout.IntField("id", _card.id));
                _card.setProp("type", Convert.ToInt32(EditorGUILayout.EnumPopup("type", _card.type)));
                if (_card.type == CardType.servant)
                {
                    _card.setProp("cost", EditorGUILayout.IntField("cost", _card.getProp<int>("cost")));
                    _card.setProp("attack", EditorGUILayout.IntField("attack", _card.getProp<int>("attack")));
                    _card.setProp("life", EditorGUILayout.IntField("life", _card.getProp<int>("life")));
                }
            }
        }
        GeneratedCardDefine _card = null;
    }
}