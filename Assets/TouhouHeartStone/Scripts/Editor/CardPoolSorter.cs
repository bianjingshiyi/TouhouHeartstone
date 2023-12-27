using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers;
using Game;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

namespace TouhouHeartstone
{
    public class CardPoolSorter
    {
        [MenuItem("Custom/Editor/排序卡池文件")]
        public static void Sort()
        {
            var filePath = EditorUtility.OpenFilePanelWithFilters("选择卡池文件", "ExternalData/CardPools", new string[] { "醉梦传说卡池文件", "ddcp" });

            if (string.IsNullOrEmpty(filePath))
                return;
            string infoJson = null;
            string json = null;
            using (var reader = new StreamReader(filePath))
            {
                infoJson = reader.ReadLine();
                json = reader.ReadToEnd();
            }
            JsonHelper.init();

            var cardPool = JsonHelper.fromJson<SerializableCardPool>(json);

            sort(cardPool.animationDefineList, a => a.id);
            sort(cardPool.actionDefineList, a => a.id);
            sort(cardPool.buffDefineList, a => a.id);
            sort(cardPool.buffSkinList, a => a.buffId);
            sort(cardPool.cardDefineList, a => a.id);
            sort(cardPool.cardSkinList, a => a._id);
            sort(cardPool.ruleLogicList, a => a.id);
            var newJson = cardPool.toJson(true);
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine(infoJson);
                writer.WriteLine(newJson);
            }

            Debug.Log("卡池排序完毕。");
        }

        private static void sort<T>(IList<T> list, Func<T, object> comparer)
        {
            if (list == null)
                return;
            var orderedList = list.OrderBy(comparer).ToArray();
            list.Clear();
            foreach (var ordered in orderedList)
            {
                list.Add(ordered);
            }
        }
    }
}
