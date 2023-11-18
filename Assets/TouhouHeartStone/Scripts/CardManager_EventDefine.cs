using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        #region 获取事件定义
        public static EventReference getEventReferenceOfAction(GeneratedActionDefine gen)
        {
            return new EventReference(gen.cardPoolId, gen.getEventName());
        }
        public static EventReference getEventReferenceOfEventArg(IEventArg arg)
        {
            if (arg is GeneratedEventArg gen)
            {
                return getEventReferenceOfAction(gen.actionDefine);
            }
            return new EventReference(0, EventHelper.getName(arg));
        }
        public EventDefine getEventDefine(EventReference eventRef, bool includeHidden = true)
        {
            if (eventRef == null)
                return null;
            var poolRef = getCardPoolReference(eventRef.cardPoolId);
            return getEventDefine(poolRef, eventRef.eventName, includeHidden);
        }
        public EventDefine getEventDefine(ICardPoolReference cardPoolRef, string name, bool includeHidden = true)
        {
            if (isBuiltinCardPool(cardPoolRef.CardPoolID))
            {
                return game.eventManager.getOrLoadBuiltinEventDefines(includeHidden).FindEventDefine(name);
            }
            else if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                return cardPool.eventDefineList.FindEventDefine(name);
            }
            return null;
        }
        public EventDefine[] getEventDefines(ICardPoolReference cardPoolRef)
        {
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

        #region 获取事件触发时间

        public EventTriggerTime getEventTriggerTime(ICardPoolReference cardPoolRef, string name, bool includeHidden = true)
        {
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                var define = cardPool.eventDefineList.getEventTriggerTimeByName(name);
                if (define != null)
                {
                    return define;
                }
            }
            var eventDefine = game.eventManager.getOrLoadBuiltinEventDefines(includeHidden).getEventTriggerTimeByName(name);
            if (eventDefine != null)
            {
                return eventDefine;
            }
            return null;
        }
        public EventReference[] getAnyActionBindedEvents(ActionReference actionRef)
        {
            if (actionRef == null)
                return null;
            return getAnyActionBindedEvents(actionRef.cardPoolId, actionRef.defineId);
        }
        public EventReference[] getAnyActionBindedEvents(long poolId, int actionId)
        {
            var poolRef = getCardPoolReference(poolId);
            return getActionBindedEvents(poolRef, actionId);
        }
        public EventReference[] getActionBindedEvents(ICardPoolReference poolRef, int actionId)
        {
            if (isBuiltinCardPool(poolRef))
            {
                return game.eventManager.getBuiltinActionBindedEvents(actionId);
            }
            else
            {
                var actionDefine = getActionDefine(poolRef, actionId);
                if (actionDefine is GeneratedActionDefine gen && actionDefine.type == NodeDefineType.Event)
                {
                    return new EventReference[] { getEventReferenceOfAction(gen) };
                }
            }
            return null;
        }
        public EventReference[] getAnyActionBindedEventsRecursive(ActionReference actionRef)
        {
            if (actionRef == null)
                return null;
            return getAnyActionBindedEventsRecursive(actionRef.cardPoolId, actionRef.defineId);
        }
        public EventReference[] getAnyActionBindedEventsRecursive(long poolId, int actionId)
        {
            var poolRef = getCardPoolReference(poolId);
            return getActionBindedEventsRecursive(poolRef, actionId);
        }
        public EventReference[] getActionBindedEventsRecursive(ICardPoolReference poolRef, int actionId)
        {
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
                game.eventManager.getEventReferenceChildrenNonAlloc(eventRef, refs);
            }
            return null;
        }
        #endregion

        #endregion

        #region 私有方法
        private EventDefine[] getEventDefines(CardPool cardPool)
        {
            return cardPool.eventDefineList.ToArray();
        }
        #endregion
    }
}
