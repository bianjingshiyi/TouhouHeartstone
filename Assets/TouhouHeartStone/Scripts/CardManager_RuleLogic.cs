using System;
using System.Collections.Generic;
using System.Linq;
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
        public GeneratedRuleGameLogic createRuleGameLogic(long cardPoolId, GameRuleLogicType type)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                var logicList = cardPool.ruleLogicList;
                //分配Id
                int id = Math.Max(0, logicList.Count > 0 ? logicList.Max(def => def.id) : 0) + 1;
                var graph = new ActionGraph();
                GeneratedRuleGameLogic ruleLogic = new GeneratedRuleGameLogic(type);
                ruleLogic.cardPoolId = cardPoolId;
                ruleLogic.id = id;

                return ruleLogic;
            }
            return null;
        }

        public GeneratedRuleEventLogic createRuleEventLogic(long cardPoolId, EventTriggerTime time)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                var logicList = cardPool.ruleLogicList;
                //分配Id
                int id = Math.Max(0, logicList.Count > 0 ? logicList.Max(def => def.id) : 0) + 1;
                var graph = new ActionGraph();
                GeneratedRuleEventLogic ruleLogic = new GeneratedRuleEventLogic(time);
                ruleLogic.cardPoolId = cardPoolId;
                ruleLogic.id = id;

                return ruleLogic;
            }
            return null;
        }

        public GeneratedRuleLogic cloneRuleLogic(long cardPoolId, GeneratedRuleLogic logic)
        {
            var json = logic.serialize().toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableGeneratedRuleLogic>(json);


            var nodeDefiner = getLocalNodeDefiner();
            // 铺设节点。
            var clone = cloneSeri.deserialize(nodeDefiner);
            clone.cardPoolId = cardPoolId;
            // 定义所有节点。
            clone.graph.DefineNodes(nodeDefiner);
            // 连接所有节点。
            var graph = clone.graph;
            var nodes = cloneSeri.graph.GetConnections(graph);
            graph.AddConnections(nodes);

            return clone;
        }
        #endregion

        #region 添加
        public void addRuleLogic(long cardPoolId, GeneratedRuleLogic ruleLogic)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                cardPool.ruleLogicList.Add(ruleLogic);
            }
        }
        #endregion

        #region 移除
        public bool removeRuleLogic(GeneratedRuleLogic ruleLogic)
        {
            if (ruleLogic == null)
                return false;
            var cardPoolRef = getLocalCardPoolReference(ruleLogic.cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.ruleLogicList.Remove(ruleLogic);
            }
            return false;
        }
        public bool removeRuleLogic(long cardPoolId, int id)
        {
            if (getLocalRuleLogic(cardPoolId, id) is not GeneratedRuleLogic gen)
                return false;
            return removeRuleLogic(gen);
        }
        #endregion

        #region 获取
        public GeneratedRuleLogic getRuleLogic(ICardPoolReference poolRef, int id)
        {
            if (tryGetCardPool(poolRef, out CardPool cardPool))
            {
                //自定义
                return cardPool.ruleLogicList.Find(g => g.id == id);
            }
            return null;
        }
        public GeneratedRuleLogic getLocalRuleLogic(long poolId, int id)
        {
            var poolRef = getLocalCardPoolReference(poolId);
            return getRuleLogic(poolRef, id);
        }
        public GeneratedRuleLogic getLocalRuleLogic(DefineReference actRef)
        {
            if (actRef == null)
                return null;
            return getLocalRuleLogic(actRef.cardPoolId, actRef.defineId);
        }
        /// <summary>
        /// 获取一个自定义卡池内的所有规则逻辑。
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <returns></returns>
        public GeneratedRuleLogic[] getRuleLogics(ICardPoolReference cardPoolRef)
        {
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.ruleLogicList.ToArray();
            }
            return new GeneratedRuleLogic[0];
        }
        /// <summary>
        /// 获取一个本地自定义卡池内的所有规则逻辑。
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <returns></returns>
        public GeneratedRuleLogic[] getLocalRuleLogics(long cardPoolId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return getRuleLogics(poolRef);
        }
        /// <summary>
        /// 获取内置的所有规则逻辑，以及几个自定义卡池内的所有规则逻辑。
        /// </summary>
        /// <param name="includedCardPools"></param>
        /// <returns></returns>
        public GeneratedRuleLogic[] getAllRuleLogics(params ICardPoolReference[] includedCardPools)
        {
            List<GeneratedRuleLogic> ruleLogicList = new List<GeneratedRuleLogic>();
            //自定义
            if (includedCardPools != null)
            {
                foreach (var poolRef in includedCardPools)
                {
                    ruleLogicList.AddRange(getRuleLogics(poolRef));
                }
            }
            return ruleLogicList.ToArray();
        }
        /// <summary>
        /// 获取内置的所有规则逻辑，以及几个本地自定义卡池内的所有规则逻辑。
        /// </summary>
        /// <param name="includedCardPools"></param>
        /// <returns></returns>
        public GeneratedRuleLogic[] getAllLocalRuleLogics(params long[] includedCardPools)
        {
            return getAllRuleLogics(includedCardPools.Select(id => getLocalCardPoolReference(id)).ToArray());
        }
        #endregion

        #endregion
    }
}
