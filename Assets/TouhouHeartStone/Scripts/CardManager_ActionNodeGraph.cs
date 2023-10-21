using Controllers;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    partial class CardManager
    {
        public ActionNodeGraphInfo getOrCreateCardEffectActionNodeGraphInfo(long cardPoolId, int cardDefineId, int effectIndex)
        {
            CardPool cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                ActionNodeGraphInfo info = new ActionNodeGraphInfo()
                {
                    cardId = cardDefineId,
                    effectIndex = effectIndex
                };
                return info;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        public ActionNodeGraphInfo getOrCreateBuffEffectActionNodeGraphInfo(long cardPoolId, int buffDefineId, int effectIndex)
        {
            CardPool cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                ActionNodeGraphInfo info = new ActionNodeGraphInfo()
                {
                    buffId = buffDefineId,
                    effectIndex = effectIndex
                };
                return info;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        public ActionNodeGraphInfo getOrCreateActionDefineActionNodeGraphInfo(long cardPoolId, int actionDefineId)
        {
            CardPool cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                var info = new ActionNodeGraphInfo()
                {
                    actionDefineId = actionDefineId
                };
                return info;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
    }
}
