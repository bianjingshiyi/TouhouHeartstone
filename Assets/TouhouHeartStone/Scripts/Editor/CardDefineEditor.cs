using System;
using System.IO;
using System.Xml;
using System.Linq;
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
                if (_card.type == CardDefineType.servant)
                {
                    _card.setProp("cost", EditorGUILayout.IntField("cost", _card.getProp<int>("cost")));
                    _card.setProp("attack", EditorGUILayout.IntField("attack", _card.getProp<int>("attack")));
                    _card.setProp("life", EditorGUILayout.IntField("life", _card.getProp<int>("life")));
                }
                //效果
                _effectScrollPosition = GUILayout.BeginScrollView(_effectScrollPosition);
                List<GeneratedEffect> effectList = new List<GeneratedEffect>(_card.getProp<Effect[]>("effects") != null ? _card.getProp<Effect[]>("effects").Cast<GeneratedEffect>() : new GeneratedEffect[0]);
                //绘制已有的效果
                for (int i = 0; i < effectList.Count; i++)
                {
                    GeneratedEffect effect = effectList[i];
                    effect.setPile(EditorGUILayout.TextField(effect.pile));
                    effect.setTrigger(EditorGUILayout.TextField(effect.trigger));
                    effect.setScript(EditorGUILayout.TextArea(effect.script));
                    //删除效果
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    if (GUILayout.Button("删除效果", GUILayout.Width(100)))
                    {
                        effectList.RemoveAt(i);
                        i--;
                    }
                    GUILayout.EndHorizontal();
                }
                foreach (GeneratedEffect effect in effectList)
                {
                }
                //添加新效果按钮
                GUILayout.BeginHorizontal();
                GUILayout.Label("");
                if (GUILayout.Button("新增效果", GUILayout.Width(100)))
                    effectList.Add(new GeneratedEffect("Field", "onUse", ""));
                GUILayout.EndHorizontal();
                if (effectList.Count > 0)
                    _card.setProp("effects", effectList.Cast<GeneratedEffect>().ToArray());
                else
                    _card.setProp<Effect[]>("effects", null);
                GUILayout.EndScrollView();
            }
        }
        Vector2 _effectScrollPosition;
        GeneratedCardDefine _card = null;
    }
}