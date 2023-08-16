using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Controllers;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager : IActionDefineGetter
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
                actionDefine.cardPoolId = cardPoolId;
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
        public GeneratedActionDefine cloneActionDefine(long cardPoolId, GeneratedActionDefine define)
        {
            var json = new SerializableActionDefine(define).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableActionDefine>(json);


            var nodeDefiner = new THHNodeDefiner(this);
            // 铺设节点。
            var clone = cloneSeri.toGeneratedActionDefine(game.editor.getTypeByName);
            clone.cardPoolId = cardPoolId;
            // 定义所有节点。
            clone.graph.DefineGeneratedActionDefine(clone);
            clone.graph.DefineNodes(nodeDefiner);
            // 连接所有节点。
            var graph = clone.graph;
            var nodes = cloneSeri.graph.GetConnections(graph);
            graph.AddConnections(nodes);

            return clone;
        }
        public ActionDefine getActionDefine(ActionReference actRef)
        {
            return getActionDefine(actRef.cardPoolId, actRef.defineId);
        }
        public ActionDefine getActionDefine(long cardPoolId, int id)
        {
            //自定义
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.Find(g => g.id == id);
            }
            //内置
            if (builtinActionDefines.TryGetValue(id, out ActionDefine actionDefine))
            {
                return actionDefine;
            }
            var obsoleteDefine = builtinActionDefines.Values.FirstOrDefault(d => d.obsoleteNames.Contains(name));
            if (obsoleteDefine != null)
            {
                return obsoleteDefine;
            }
            return null;
        }
        public ActionReference getBuiltinActionReferenceFromName(string name)
        {
            var builtin = builtinActionDefines.Values.FirstOrDefault(d => d.obsoleteNames.Contains(name));
            if (builtin != null)
            {
                return new ActionReference(0, builtin.defineId);
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
        public ActionDefine[] getAllActionDefines(params long[] includedCardPools)
        {
            //内置
            List<ActionDefine> actionDefineList = new List<ActionDefine>();
            foreach (var pair in builtinActionDefines)
            {
                actionDefineList.Add(pair.Value);
            }
            //自定义
            if (includedCardPools != null)
            {
                foreach (long poolId in includedCardPools)
                {
                    actionDefineList.AddRange(getActionDefines(poolId));
                }
            }
            return actionDefineList.ToArray();
        }
        #endregion
        private void onAwake_Actions()
        {
            // 加载内置动作定义
            builtinActionDefines = ActionDefine.loadDefinesFromAssemblies(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            });
        }
        public Dictionary<int, ActionDefine> builtinActionDefines { get; private set; }
    }
}
