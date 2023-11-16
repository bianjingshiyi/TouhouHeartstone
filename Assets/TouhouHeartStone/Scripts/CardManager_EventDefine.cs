using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
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
        public EventReference[] getActionBindedEvents(ActionReference actionRef)
        {
            if (isBuiltinCardPool(actionRef.cardPoolId))
            {
                return game.eventManager.getBuiltinActionBindedEvents(actionRef.defineId);
            }
            else
            {
                var actionDefine = getActionDefine(actionRef);
                if (actionDefine is GeneratedActionDefine gen && actionDefine.type == NodeDefineType.Event)
                {
                    return new EventReference[] { getEventReferenceOfAction(gen) };
                }
            }
            return null;
        }
        public EventReference[] getActionBindedEventsRecursive(ActionReference actionRef)
        {
            if (isBuiltinCardPool(actionRef.cardPoolId))
            {
                return game.eventManager.getBuiltinActionBindedEventsRecursive(actionRef.defineId);
            }
            else if (getActionDefine(actionRef) is GeneratedActionDefine gen && gen.type == NodeDefineType.Event)
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
        #region 私有方法
        private EventDefine[] getEventDefines(CardPool cardPool)
        {
            return cardPool.eventDefineList.ToArray();
        }
        #endregion
    }
}
