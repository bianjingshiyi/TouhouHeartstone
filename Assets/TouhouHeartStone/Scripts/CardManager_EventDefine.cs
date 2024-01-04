using System;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        #region 获取事件定义
        public static EventReference getEventReferenceOfGeneratedAction(ActionReference gen)
        {
            return new EventReference(gen.cardPoolId, GeneratedActionDefine.getEventName(gen));
        }
        public static EventReference getEventReferenceOfAction(GeneratedActionDefine gen)
        {
            return new EventReference(gen.cardPoolId, gen.getEventName());
        }
        public static EventReference getEventReferenceOfEventArg(IEventArg arg)
        {
            if (arg == null || arg.define == null)
                return null;
            if (arg.define is GeneratedEventDefine gen)
            {
                return getEventReferenceOfGeneratedAction(gen.actionReference);
            }
            return new EventReference(0, EventHelper.getName(arg.define));
        }

        #region 获取单个事件定义
        public EventDefine getEventDefine(ICardPoolReference cardPoolRef, string name, bool includeHidden = true)
        {
            if (cardPoolRef == null)
                return null;
            if (isBuiltinCardPool(cardPoolRef))
            {
                return game.eventManager.getOrLoadBuiltinEventDefines(includeHidden).FindEventDefine(name);
            }
            else if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                return cardPool.eventDefineList.FindEventDefine(name);
            }
            return null;
        }
        public EventDefine getLocalEventDefine(EventReference eventRef, bool includeHidden = true)
        {
            return getEventDefineInEnv(eventRef, getLocalCardPoolReferences(), includeHidden);
        }
        public EventDefine getEventDefineInEnv(EventReference eventRef, IEnumerable<ICardPoolReference> env, bool includeHidden = true)
        {
            if (eventRef == null)
                return null;
            var poolRef = getCardPoolReferenceInEnv(eventRef.cardPoolId, env);
            return getEventDefine(poolRef, eventRef.eventName, includeHidden);
        }
        #endregion

        #region 获取多个事件定义
        public EventDefine[] getEventDefines(ICardPoolReference cardPoolRef)
        {
            if (cardPoolRef == null)
                return Array.Empty<EventDefine>();
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                return getEventDefines(cardPool);
            }
            return Array.Empty<EventDefine>();
        }
        public EventDefine[] getAllEventDefines(bool includeHidden = true, params ICardPoolReference[] includedCardPools)
        {
            //内置
            List<EventDefine> actionDefineList = new List<EventDefine>();
            actionDefineList.AddRange(game.eventManager.getOrLoadBuiltinEventDefines(includeHidden));
            //自定义
            if (includedCardPools != null)
            {
                foreach (var poolRef in includedCardPools)
                {
                    actionDefineList.AddRange(getEventDefines(poolRef));
                }
            }
            return actionDefineList.ToArray();
        }
        #endregion

        #endregion

        #region 获取事件触发时间

        public EventTriggerTime getEventTriggerTime(ICardPoolReference cardPoolRef, string name, bool includeHidden = true)
        {
            if (cardPoolRef == null)
                return null;
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                var triggerTime = cardPool.eventDefineList.getEventTriggerTimeByName(name);
                if (triggerTime != null)
                {
                    return triggerTime;
                }
            }
            var eventDefine = game.eventManager.getOrLoadBuiltinEventDefines(includeHidden).getEventTriggerTimeByName(name);
            if (eventDefine != null)
            {
                return eventDefine;
            }
            return null;
        }
        public EventReference[] getLocalActionBindedEventsRecursive(ActionReference actionRef)
        {
            if (actionRef == null)
                return null;
            return getLocalActionBindedEventsRecursive(actionRef.cardPoolId, actionRef.defineId);
        }
        public EventReference[] getLocalActionBindedEventsRecursive(long poolId, int actionId)
        {
            return getActionBindedEventsRecursive(poolId, actionId, getLocalCardPoolReferences());
        }
        public EventReference[] getActionBindedEventsRecursive(ActionReference actionRef, IEnumerable<ICardPoolReference> poolSet)
        {
            if (actionRef == null)
                return null;
            return getActionBindedEventsRecursive(actionRef.cardPoolId, actionRef.defineId, poolSet);
        }
        public EventReference[] getActionBindedEventsRecursive(long poolId, int actionId, IEnumerable<ICardPoolReference> poolSet)
        {
            var poolRef = getCardPoolReferenceInEnv(poolId, poolSet);
            if (isBuiltinCardPool(poolRef))
            {
                return game.eventManager.getBuiltinActionBindedEventsRecursive(actionId);
            }
            else if (getActionDefine(poolRef, actionId) is GeneratedActionDefine gen && gen.type == NodeDefineType.Event)
            {
                var eventRef = getEventReferenceOfAction(gen);
                List<EventReference> refs = new List<EventReference>
                {
                    eventRef
                };
                game.eventManager.getEventReferenceChildrenNonAlloc(eventRef, poolSet, refs);
            }
            return null;
        }
        #endregion

        #endregion

        #region 私有方法

        #region 添加事件定义
        private void addEventDefine(long cardPoolId, EventDefine define)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                cardPool.eventDefineList.Add(define);
            }
        }
        private void removeEventDefine(long cardPoolId, string eventName)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                cardPool.eventDefineList.RemoveAll(e => e.eventName == eventName);
            }
        }
        #endregion
        private EventDefine[] getEventDefines(CardPool cardPool)
        {
            return cardPool.eventDefineList.ToArray();
        }
        #endregion
    }
}
