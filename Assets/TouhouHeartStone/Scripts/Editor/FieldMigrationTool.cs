using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Game.Animations;
using System;
using static UnityEngine.ParticleSystem;

namespace TouhouHeartstone
{
    public class FieldMigrationTool : EditorWindow
    {
        [MenuItem("Tools/Field Migration Tool")]
        public static void ShowWindow()
        {
            GetWindow<FieldMigrationTool>("Field Migration Tool");
        }

        private void OnGUI()
        {
            if (GUILayout.Button($"开始迁移项目中所有{compoType.Name}的{oldFieldName}数据"))
            {
                MigrateFields();
            }
        }

        private void MigrateFields()
        {
            int migratedCount = 0;

            // 先处理所有Prefab
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool changed = false;
                Component[] components = prefab.GetComponentsInChildren(compoType, true);
                foreach (Component comp in components)
                {
                    if (MigrateComponent(comp))
                    {
                        changed = true;
                        migratedCount++;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"迁移Prefab: {path}");
                }
            }

            // 处理当前打开场景的实例
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var comps = root.GetComponentsInChildren<ProjectileEffect>(true);
                    foreach (var comp in comps)
                    {
                        if (MigrateComponent(comp))
                        {
                            migratedCount++;
                            EditorUtility.SetDirty(comp);
                        }
                    }
                }
            }

            if (migratedCount > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"迁移完成，共迁移 {migratedCount} 个组件数据。");
            }
            else
            {
                Debug.Log("未发现需要迁移的数据。");
            }
        }

        private bool MigrateComponent(Component comp)
        {
            if (comp == null) return false;

            // 利用反射访问私有字段或你可以改成public或[SerializeField]
            var type = typeof(ProjectileEffect);
            var oldField = type.GetField(oldFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var newField = type.GetField(newFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (oldField == null || newField == null) return false;

            object oldValue = oldField.GetValue(comp);
            object newValue = ConvertOldValue(oldValue);

            newField.SetValue(comp, newValue);
            // 可选：清空旧字段，防止重复迁移
            oldField.SetValue(comp, null);
            return true;
        }

        private object ConvertOldValue(object oldValue)
        {
            if (oldValue is MinMaxCurve minMaxCurve)
            {
                return minMaxCurve;
            }
            else if (oldValue is AnimationCurve curve)
            {
                MinMaxCurve newCurve = new MinMaxCurve(1, curve);
                return newCurve;
            }
            return newFieldType.IsValueType ? Activator.CreateInstance(newFieldType) : null;
        }

        private static Type compoType = typeof(ProjectileEffect);
        private static Type oldFieldType = typeof(AnimationCurve);
        private static Type newFieldType = typeof(MinMaxCurve);
        private static string oldFieldName = "_newOffsetCurve";
        private static string newFieldName = "_offsetCurve";
    }
}