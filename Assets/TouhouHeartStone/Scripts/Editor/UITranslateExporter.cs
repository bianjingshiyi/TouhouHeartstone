using System.IO;
using Controllers;
using Game;
using MukioI18n;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone
{
    public class UITranslateExporter
    {
        #region 菜单项
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

            potGenerator.WriteOut(System.IO.Path.Combine(FileLoader.FolderPath, "i18n", "ui", "zmcs.pot"));
            Debug.Log("翻译文件写出完毕");
            EditorSceneManager.OpenScene(active);
        }

        [MenuItem("Custom/Translate/查找代码重复翻译")]
        public static void TestGenerateCode()
        {
            MukioPotGenerator potGenerator = new MukioPotGenerator("ZMCS", "Translation Team");
            TranslateMsgAttributeFinder.FindAll(potGenerator);
        }
        [MenuItem("Custom/Translate/导出经典卡池翻译")]
        public static void ExportClassicCardPool()
        {
            MukioPotGenerator potGenerator = new MukioPotGenerator("ZMCS", "Translation Team");

            var cardPoolPath = Path.Combine(FileLoader.FolderPath, "CardPools", "Classic.ddcp");
            ExportFromCardPool(potGenerator, cardPoolPath);

            potGenerator.WriteOut(cardPoolPotPath);
            Debug.Log("翻译文件导出完毕");
        }
        #endregion

        #region UI
        static void SearchObject(Transform tr, MukioPotGenerator pot)
        {
            var cp = tr.GetComponent<ITranslateComponent>();
            addTranslate(pot, cp);

            for (int i = 0; i < tr.childCount; i++)
            {
                SearchObject(tr.GetChild(i), pot);
            }
        }
        private static void addTranslate(MukioPotGenerator pot, ITranslateComponent cp)
        {
            if (cp == null)
                return;
            if (cp.Key != null)
            {
                PotTranslate translate = new PotTranslate(cp.Key, cp.Path, cp.Comment, cp.Context);
                pot.AddString(translate);
            }
            if (cp.Keys != null)
            {
                foreach (var key in cp.Keys)
                {
                    PotTranslate translate = new PotTranslate(key, cp.Path, cp.Comment, cp.Context);
                    pot.AddString(translate);
                }
            }
        }
        #endregion

        #region 卡池
        private static void ExportFromCardPool(MukioPotGenerator pot, string path)
        {
            string infoJson = null;
            string json = null;
            using (var reader = new StreamReader(path))
            {
                infoJson = reader.ReadLine();
                json = reader.ReadToEnd();
            }
            JsonHelper.init();

            var cardPoolInfo = JsonHelper.fromJson<SerializableCardPoolInfo>(infoJson);
            var cardPool = JsonHelper.fromJson<SerializableCardPool>(json);

            var cardNameContext = CardManager.cardNameContext;
            var cardSubnameContext = CardManager.cardSubnameContext;
            var cardDescContext = CardManager.cardDescContext;
            var buffNameContext = CardManager.buffNameContext;
            var buffDescContext = CardManager.buffDescContext;
            foreach (var skin in cardPool.cardSkinList)
            {
                var reference = $"{cardPoolInfo.cardPoolName}/卡牌/{skin._name}";
                var cardComment = $"卡牌“{skin._name}”";
                if (!string.IsNullOrEmpty(skin._name)) pot.AddString(new PotTranslate(skin._name, reference, cardComment, cardNameContext));
                if (!string.IsNullOrEmpty(skin._subname)) pot.AddString(new PotTranslate(skin._subname, reference, cardComment, cardSubnameContext));
                if (!string.IsNullOrEmpty(skin._desc)) pot.AddString(new PotTranslate(skin._desc, reference, cardComment, cardDescContext));
            }
            foreach (var skin in cardPool.buffSkinList)
            {
                var reference = $"{cardPoolInfo.cardPoolName}/增益/{skin.name}";
                var buffComment = $"增益{skin.name}";
                if (!string.IsNullOrEmpty(skin.name)) pot.AddString(new PotTranslate(skin.name, reference, buffComment, buffNameContext));
                if (!string.IsNullOrEmpty(skin.desc)) pot.AddString(new PotTranslate(skin.desc, reference, buffComment, buffDescContext));
            }
        }
        const string cardPoolPotPath = "ExternalData/i18n/cards/classic.pot";
        #endregion
    }
}
