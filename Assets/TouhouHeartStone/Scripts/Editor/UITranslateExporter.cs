using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using MukioI18n;

namespace TouhouHeartstone {
    public class UITranslateExporter
    {
        [MenuItem("Custom/Translate/导出UI翻译")]
        public static void Generate()
        {
            MukioPotGenerator potGenerator = new MukioPotGenerator("ZMCS", "Translation Team");

            var active = SceneManager.GetActiveScene().path;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                var sc = EditorSceneManager.OpenScene(scene.path);
                if (sc == null) continue;
                var objects = sc.GetRootGameObjects();
                foreach (var item in objects)
                {
                    SearchObject(item.transform, potGenerator);
                }
            }

            TranslateMsgAttributeFinder.FindAll(potGenerator);

            potGenerator.WriteOut(System.IO.Path.Combine(Application.streamingAssetsPath, "i18n", "ui", "zmcs.pot"));
            Debug.Log("翻译文件写出完毕");
            EditorSceneManager.OpenScene(active);
        }

        static void SearchObject(Transform tr, MukioPotGenerator pot)
        {
            var cp = tr.GetComponent<ITranslateComponent>();
            if (cp != null)
                pot.AddString(cp.Key, cp.Path);

            for (int i = 0; i < tr.childCount; i++)
            {
                SearchObject(tr.GetChild(i), pot);
            }
        }
    }
}
