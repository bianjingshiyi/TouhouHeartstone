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
        public EventDefine getEventDefine(ICardPoolReference cardPoolRef, string name, bool includeHidden = true)
        {
            if (cardPoolRef == null)
                return null;
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
        public EventDefine getLocalEventDefine(long cardPoolId, string name, bool includeHidden = true)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return getEventDefine(poolRef, name, includeHidden);
        }
        public EventDefine getLocalEventDefine(EventReference eventRef, bool includeHidden = true)
        {
            if (eventRef == null)
                return null;
            return getLocalEventDefine(eventRef.cardPoolId, eventRef.eventName, includeHidden);
        }
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

        #region 获取事件触发时间

        public EventTriggerTime getEventTriggerTime(ICardPoolReference cardPoolRef, string name, bool includeHidden = true)
        {
            if (cardPoolRef == null)
                return null;
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
            var poolRef = getCardPoolReferenceInSet(poolId, poolSet);
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
        private EventDefine[] getEventDefines(CardPool cardPool)
        {
            return cardPool.eventDefineList.ToArray();
        }
        #endregion
    }
}
