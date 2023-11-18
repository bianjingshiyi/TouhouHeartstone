using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Controllers;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        #region 创建
        public GeneratedActionDefine createActionDefine(long cardPoolId, string name, NodeDefineType type)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                var actDefList = cardPool.actionDefineList;
                //分配Id
                int id = Math.Max(0, actDefList.Max(def => def.defineId)) + 1;
                var graph = new ActionGraph();
                GeneratedActionDefine actionDefine = new GeneratedActionDefine(graph, id, type, null, name, null, null, null);
                actionDefine.cardPoolId = cardPoolId;
                actionDefine.InitNodes();
                addActionDefine(cardPoolId, actionDefine);

                return actionDefine;
            }
            return null;
        }

        public GeneratedActionDefine cloneActionDefine(long cardPoolId, GeneratedActionDefine define)
        {
            var json = new SerializableActionDefine(define).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableActionDefine>(json);


            var nodeDefiner = getLocalNodeDefiner();
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
        #endregion

        #region 添加
        public void addActionDefine(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                cardPool.actionDefineList.Add(actionDefine);
                if (actionDefine.type == NodeDefineType.Event)
                {
                    cardPool.eventDefineList.Add(game.eventManager.generateEventDefineForActionDefine(actionDefine));
                }
            }
        }
        #endregion

        #region 移除
        public bool removeActionDefine(GeneratedActionDefine actionDefine)
        {
            if (actionDefine == null)
                return false;
            var cardPoolRef = getLocalCardPoolReference(actionDefine.cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                if (cardPool.actionDefineList.Remove(actionDefine))
                {
                    if (actionDefine.type == NodeDefineType.Event)
                    {
                        cardPool.eventDefineList.RemoveAll(e => e.eventName == actionDefine.getEventName());
                    }
                }
            }
            return false;
        }
        public bool removeActionDefine(long cardPoolId, int id)
        {
            if (getLocalActionDefine(cardPoolId, id) is not GeneratedActionDefine gen)
                return false;
            return removeActionDefine(gen);
        }
        #endregion

        #region 获取
        public ActionDefine getActionDefine(ICardPoolReference poolRef, int id)
        {
            if (tryGetCardPool(poolRef, out CardPool cardPool))
            {
                //自定义
                return cardPool.actionDefineList.Find(g => g.defineId == id);
            }
            else if (isBuiltinCardPool(poolRef.CardPoolID))
            {
                //内置
                if (builtinActionDefines.TryGetValue(id, out ActionDefine actionDefine))
                    return actionDefine;
                // 过时内置。
                var obsoleteDefine = builtinActionDefines.Values.FirstOrDefault(d => d.obsoleteNames.Contains(name));
                if (obsoleteDefine != null)
                    return obsoleteDefine;
            }
            return null;
        }
        public ActionDefine getLocalActionDefine(long poolId, int id)
        {
            var poolRef = getLocalCardPoolReference(poolId);
            return getActionDefine(poolRef, id);
        }
        public ActionDefine getLocalActionDefine(ActionReference actRef)
        {
            if (actRef == null)
                return null;
            return getLocalActionDefine(actRef.cardPoolId, actRef.defineId);
        }
        /// <summary>
        /// 根据名称获取过时的内置动作定义。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionReference getBuiltinActionReferenceFromName(string name)
        {
            var builtin = builtinActionDefines.Values.FirstOrDefault(d => d.obsoleteNames.Contains(name));
            if (builtin != null)
                return new ActionReference(0, builtin.defineId);
            return null;
        }
        /// <summary>
        /// 获取一个自定义卡池内的所有动作定义。
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <returns></returns>
        public GeneratedActionDefine[] getActionDefines(ICardPoolReference cardPoolRef)
        {
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.ToArray();
            }
            return new GeneratedActionDefine[0];
        }
        /// <summary>
        /// 获取一个本地自定义卡池内的所有动作定义。
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <returns></returns>
        public GeneratedActionDefine[] getLocalActionDefines(long cardPoolId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return getActionDefines(poolRef);
        }
        /// <summary>
        /// 获取内置的所有动作定义，以及几个自定义卡池内的所有动作定义。
        /// </summary>
        /// <param name="includedCardPools"></param>
        /// <returns></returns>
        public ActionDefine[] getAllActionDefines(params ICardPoolReference[] includedCardPools)
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
                foreach (var poolRef in includedCardPools)
                {
                    actionDefineList.AddRange(getActionDefines(poolRef));
                }
            }
            return actionDefineList.ToArray();
        }
        /// <summary>
        /// 获取内置的所有动作定义，以及几个本地自定义卡池内的所有动作定义。
        /// </summary>
        /// <param name="includedCardPools"></param>
        /// <returns></returns>
        public ActionDefine[] getAllLocalActionDefines(params long[] includedCardPools)
        {
            return getAllActionDefines(includedCardPools.Select(id => getLocalCardPoolReference(id)).ToArray());
        }
        #endregion

        #endregion

        #region 私有方法
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
        #endregion

        #region 属性字段
        public Dictionary<int, ActionDefine> builtinActionDefines { get; private set; }
        #endregion
    }
}
