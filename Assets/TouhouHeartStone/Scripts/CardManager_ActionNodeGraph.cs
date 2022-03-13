using Controllers;
using System.Collections.Generic;

namespace Game
{
    partial class CardManager
    {
        public ActionNodeGraphInfo getOrCreateCardEffectActionNodeGraphInfo(int cardPoolId, int cardDefineId, int effectIndex, string entryType, int triggerIndex)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                ActionNodeGraphInfo info = cardPool.actionNodeGraphInfoList.Find(a =>
                    a.cardId == cardDefineId &&
                    a.effectIndex == effectIndex &&
                    a.entryType == entryType &&
                    a.triggerIndex == triggerIndex);
                if (info == null)
                {
                    info = new ActionNodeGraphInfo()
                    {
                        cardId = cardDefineId,
                        effectIndex = effectIndex,
                        entryType = entryType,
                        triggerIndex = triggerIndex
                    };
                    cardPool.actionNodeGraphInfoList.Add(info);
                }
                return info;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        public ActionNodeGraphInfo getOrCreateBuffEffectActionNodeGraphInfo(int cardPoolId, int buffDefineId, int effectIndex, string entryType, int triggerIndex)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                ActionNodeGraphInfo info = cardPool.actionNodeGraphInfoList.Find(a =>
                    a.buffId == buffDefineId &&
                    a.effectIndex == effectIndex &&
                    a.entryType == entryType &&
                    a.triggerIndex == triggerIndex);
                if (info == null)
                {
                    info = new ActionNodeGraphInfo()
                    {
                        buffId = buffDefineId,
                        effectIndex = effectIndex,
                        entryType = entryType,
                        triggerIndex = triggerIndex
                    };
                    cardPool.actionNodeGraphInfoList.Add(info);
                }
                return info;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
    }
}
