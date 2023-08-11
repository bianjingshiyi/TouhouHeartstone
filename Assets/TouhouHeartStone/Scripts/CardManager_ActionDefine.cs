using System.Collections.Generic;
using System.Linq;
using Controllers;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public GeneratedActionDefine createActionDefine(long cardPoolId, string name)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                var actDefList = cardPool.actionDefineList;
                //分配Id
                int id = 0;
                for (int i = 0; i < actDefList.Count; i++)
                {
                    if (actDefList[i].id > id)
                    {
                        id = actDefList[i].id;
                    }
                }
                id++;
                var graph = new ActionGraph();
                GeneratedActionDefine actionDefine = new GeneratedActionDefine(graph, id, null, name, null, null, null);
                actionDefine.InitNodes();
                addActionDefine(cardPoolId, actionDefine);

                return actionDefine;
            }
            return null;
        }
        public void addActionDefine(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                cardPool.actionDefineList.Add(actionDefine);
            }
        }
        public bool removeActionDefine(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.Remove(actionDefine);
            }
            return false;
        }
        public bool removeActionDefine(long cardPoolId, int id)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.RemoveAll(a => a.id == id) > 0;
            }
            return false;
        }
        public bool getActionDefine(long cardPoolId, string name, out GeneratedActionDefine actionDefine)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                actionDefine = cardPool.actionDefineList.Find(g => g.defineName == name);
                return actionDefine != null;
            }
            actionDefine = null;
            return false;
        }
        public GeneratedActionDefine cloneActionDefine(long cardPoolId, GeneratedActionDefine define)
        {
            var json = new SerializableActionDefine(define).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableActionDefine>(json);

            // 铺设节点。
            var clone = cloneSeri.toGeneratedActionDefine(game.editor.getTypeByName);
            // 定义所有节点。
            clone.graph.DefineGeneratedActionDefine(clone);

            EventTypeInfoFinder eventFinder = EventTypeInfoLoader.FindEventTypeInfo;
            var nodeDefiner = new THHNodeDefiner(name => game.getActionDefine(name, cardPoolId), eventFinder);
            clone.graph.DefineNodes(nodeDefiner);
            // 连接所有节点。
            var graph = clone.graph;
            var nodes = cloneSeri.graph.GetConnections(graph);
            graph.AddConnections(nodes);

            return clone;
        }
        public GeneratedActionDefine getActionDefine(long cardPoolId, int id)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.Find(g => g.id == id);
            }
            return null;
        }
        public GeneratedActionDefine[] getActionDefines(long cardPoolId)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.ToArray();
            }
            return new GeneratedActionDefine[0];
        }
        #endregion
    }
}
