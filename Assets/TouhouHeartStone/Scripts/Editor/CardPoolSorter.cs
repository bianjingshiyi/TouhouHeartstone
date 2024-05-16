using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers;
using Game;
using SimpleJSON;
using TouhouCardEngine;
using UnityEditor;
using UnityEditor.Graphs;
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
            sort(cardPool.localizedStringList, a => a.value);
            sort(cardPool.localizedResourceList, a => a.value);

            sortGraph(cardPool.actionDefineList, a => a.graph);
            sortGraph(cardPool.buffDefineList, a => a.effects.Select(e => e.graph));
            sortGraph(cardPool.cardDefineList, a => a.effects.Select(e => e.graph));
            sortGraph(cardPool.ruleLogicList, a => a.graph);

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
        private static void sortGraph<T>(IEnumerable<T> list, Func<T, SerializableActionNodeGraph> graphGetter)
        {
            if (list == null)
                return;
            foreach (var element in list)
            {
                var graph = graphGetter(element);

                sortGraph(graph);
            }
        }
        private static void sortGraph<T>(IEnumerable<T> list, Func<T, IEnumerable<SerializableActionNodeGraph>> graphGetter)
        {
            if (list == null)
                return;
            foreach (var element in list)
            {
                var graphs = graphGetter(element);

                foreach (var graph in graphs)
                {
                    sortGraph(graph);
                }
            }
        }
        private static void sortGraph(SerializableActionNodeGraph graph)
        {
            if (graph == null)
                return;
            var orderedList = graph.nodes.OrderBy(n => n.id).ToArray();
            graph.nodes.Clear();
            graph.nodes.AddRange(orderedList);

            var orderedConnections = graph.connections
                .OrderBy(n => n.sourceNodeId)
                .ThenBy(n => n.destNodeId)
                .ThenBy(n => n.sourceName)
                .ThenBy(n => n.destName)
                .ToArray();
            graph.connections.Clear();
            graph.connections.AddRange(orderedConnections);
        }
    }
}
