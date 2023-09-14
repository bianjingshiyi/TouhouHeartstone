using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone
{
    public class ResourceCleaner
    {
        [MenuItem("Custom/Editor/检查未使用资源")]
        public static void CheckUnusedResources()
        {
            check();
        }

        static string[] blacklist =
        {
            "TouhouHeartStone/MassResources/**",
            "Plugins/**",
            "StreamingAssets/**",
            "Tests/**",
            "*.cs",
        };

        static void check()
        {
            var assetPaths = AssetDatabase.GetAllAssetPaths().Where(p => {
                var path = p.Replace('\\', '/');
                return path.StartsWith("Assets/") && !Directory.Exists(path);
            });
            var dependencies = new List<string>();

            // 场景。
            var activeScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                activeScenes.Add(SceneManager.GetSceneAt(i).path);
            }
            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                var scene = EditorSceneManager.OpenScene(editorScene.path);
                addDeps(scene.path);
                EditorSceneManager.CloseScene(scene, true);
            }
            foreach (var scene in activeScenes)
            {
                EditorSceneManager.OpenScene(scene);
            }


            var resourceAssets = assetPaths.Where(p => p.Replace('\\', '/').Contains("/Resources/"));
            foreach (var resource in resourceAssets)
            {
                addDeps(resource);
            }



            var blacklistFiles = blacklist.SelectMany(b => {
                var rootPath = Application.dataPath;
                var files = Directory.GetFiles(rootPath, b, SearchOption.AllDirectories);
                return files.Select(f => f.Replace(rootPath, "Assets").Replace("\\", "/"));
            });
            var unused = assetPaths.Except(dependencies).Except(blacklistFiles).ToList();
            unused.Sort();
            Debug.Log($"Found {unused.Count} unused assets.");
            Debug.Log(string.Join("\n", unused));

            void addDeps(string path)
            {
                var deps = AssetDatabase.GetDependencies(path);
                foreach (var dep in deps)
                {
                    if (dependencies.Contains(dep))
                        continue;
                    dependencies.Add(dep);
                }
            }
        }
    }
}
