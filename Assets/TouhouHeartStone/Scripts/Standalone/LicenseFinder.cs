using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UI;
using System.Linq;

namespace IGensoukyo.Utilities
{
    [CreateAssetMenu(fileName = "LICENSE.asset", menuName = "Custom/LICENSE")]
    public class LicenseFinder : ScriptableObject
    {
        [SerializeField]
        License[] licenses = new License[0];

        public License[] Licenses => licenses;

        public void Reload()
        {
            Dictionary<string, License> dic = new Dictionary<string, License>();
            foreach (var item in licenses)
            {
                dic.Add(item.Path, item);
            }

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles("Assets", "LICENSE", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles("Assets", "LICENSE.md", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles("Assets", "LICENSE.txt", SearchOption.AllDirectories));

            var currentDir = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

            foreach (var file in files)
            {
                var path = file.Replace(currentDir, "");
                var data = File.ReadAllText(file);
                if (dic.ContainsKey(path))
                {
                    var item = dic[path];
                    item.Content = data;
                } else
                {
                    var item = new License()
                    {
                        Path = path,
                        Content = data,
                        Title = Path.GetFileName(Path.GetDirectoryName(path))
                    };
                    dic.Add(item.Path, item);
                }
            }

            licenses = dic.Values.ToArray();
        }
    }

    [Serializable]
    public struct License
    {
        public string Title;
        public string Path;
        [TextArea]
        public string Content;
    }

    [CustomEditor(typeof(LicenseFinder))]
    public class TestScriptableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (LicenseFinder)target;

            if (GUILayout.Button("刷新", GUILayout.Height(40)))
            {
                script.Reload();
            }
        }
    }
}

 
