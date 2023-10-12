using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Shared;

namespace Game
{
    partial class CardManager
    {
        public async Task<EventDefine[]> getOrLoadEventDefines(ICardPoolReference cardPoolRef)
        {
            var (_, cardPool) = await getOrLoadCardPool(cardPoolRef, true);
            return getEventDefines(cardPool);
        }
        public EventDefine getLocalEventDefine(long cardPoolId, string name)
        {
            var localRef = getLocalCardPoolReference(cardPoolId);
            return getEventDefine(localRef, name);
        }
        public EventDefine getEventDefine(ICardPoolReference cardPoolRef, string name)
        {
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                var define = cardPool.eventDefineList.FindEventDefine(name);
                if (define != null)
                {
                    return define;
                }
            }
            var eventDefine = game.eventManager.getOrLoadBuiltinEventDefines().FindEventDefine(name);
            if (eventDefine != null)
            {
                return eventDefine;
            }
            return null;
        }
        public EventDefine[] getLocalEventDefines(long cardPoolId)
        {
            var localRef = getLocalCardPoolReference(cardPoolId);
            return getEventDefines(localRef);
        }
        public EventDefine[] getEventDefines(ICardPoolReference cardPoolRef)
        {
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                return getEventDefines(cardPool);
            }
            return Array.Empty<EventDefine>();
        }
        public EventDefine[] getAllEventDefines(params long[] includedCardPools)
        {
            //内置
            List<EventDefine> actionDefineList = new List<EventDefine>();
            actionDefineList.AddRange(game.eventManager.getOrLoadBuiltinEventDefines());
            //自定义
            if (includedCardPools != null)
            {
                foreach (long poolId in includedCardPools)
                {
                    actionDefineList.AddRange(getLocalEventDefines(poolId));
                }
            }
            return actionDefineList.ToArray();
        }
        private EventDefine getEventDefine(CardPool cardPool, string eventName)
        {
            return cardPool.eventDefineList.FindEventDefine(eventName);
        }
        private EventDefine[] getEventDefines(CardPool cardPool)
        {
            return cardPool.eventDefineList.ToArray();
        }
        public EventTriggerTime getLocalEventTriggerTime(long cardPoolId, string name)
        {
            var localRef = getLocalCardPoolReference(cardPoolId);
            return getEventTriggerTime(localRef, name);
        }
        public EventTriggerTime getEventTriggerTime(ICardPoolReference cardPoolRef, string name)
        {
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                var define = cardPool.eventDefineList.getEventTriggerTimeByName(name);
                if (define != null)
                {
                    return define;
                }
            }
            var eventDefine = game.eventManager.getOrLoadBuiltinEventDefines().getEventTriggerTimeByName(name);
            if (eventDefine != null)
            {
                return eventDefine;
            }
            return null;
        }
    }
}
