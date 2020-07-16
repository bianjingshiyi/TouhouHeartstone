using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IGensoukyo
{
    public class DataSetPackerConfigWindow : EditorWindow
    {
        [MenuItem("Custom/Data/Excel转换器配置")]
        static void Init()
        {
            var window = GetWindow<DataSetPackerConfigWindow>();
            window.titleContent = new GUIContent("Excel转换器配置");

            window.Show();
        }

        Vector2 scrollPos;

        List<PackerConfig> configs = new List<PackerConfig>();

        void OnGUI()
        {
            GUILayout.Label("Excel转换器配置", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginVertical();
            {
                int? removeIndex = null;
                for (int i = 0; i < configs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("-")) removeIndex = i;
                        configs[i].ExcelFile = EditorGUILayout.TextField(configs[i].ExcelFile);
                        configs[i].DataSetFile = EditorGUILayout.TextField(configs[i].DataSetFile);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (removeIndex != null)
                    configs.RemoveAt(removeIndex.Value);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加"))
            {
                string filename = EditorUtility.OpenFilePanelWithFilters("选择Excel文件", Application.streamingAssetsPath, new string[] { "xls", "xlsx,xls" });
                if (!String.IsNullOrEmpty(filename))
                {
                    Uri streaming = new Uri(Application.streamingAssetsPath + "/");
                    Uri abs = new Uri(filename);
                    string file = streaming.MakeRelativeUri(abs).ToString();

                    configs.Add(new PackerConfig() { ExcelFile = file, DataSetFile = file + ".dataset" });
                }
            }

            if (GUILayout.Button("载入"))
            {
                configs.Clear();
                configs.AddRange(ExcelDataSetPacker.ReadConfigs());
            }

            if (GUILayout.Button("保存"))
            {
                ExcelDataSetPacker.WriteConfigs(configs.ToArray());
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
