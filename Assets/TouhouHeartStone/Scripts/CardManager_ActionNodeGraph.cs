using Controllers;
using System.Collections.Generic;

namespace Game
{
    partial class CardManager
    {
        public ActionNodeGraphInfo getOrCreateActionNodeGraphInfo(int cardPoolId, int cardDefineId, int effectIndex)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                ActionNodeGraphInfo info = cardPool.actionNodeGraphInfoList.Find(a => a.cardId == cardDefineId && a.effectIndex == effectIndex);
                if (info == null)
                {
                    info = new ActionNodeGraphInfo(cardDefineId, effectIndex);
                    cardPool.actionNodeGraphInfoList.Add(info);
                }
                return info;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
    }
}
