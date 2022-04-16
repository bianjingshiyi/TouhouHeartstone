using System.Collections.Generic;
using TouhouCardEngine;
namespace Game
{
    partial class CardManager
    {
        public SerializableActionDefine createActionDefine(int cardPoolId)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            //分配Id
            int id = 0;
            for (int i = 0; i < cardPool.actionDefineList.Count; i++)
            {
                if (cardPool.actionDefineList[i].id > id)
                {
                    id = cardPool.actionDefineList[i].id;
                }
            }
            id++;
            SerializableActionDefine actionDefine = new SerializableActionDefine()
            {
                id = id
            };
            cardPool.actionDefineList.Add(actionDefine);
            return actionDefine;
        }
        public bool removeActionDefine(int cardPoolId, SerializableActionDefine actionDefine)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            return cardPool.actionDefineList.Remove(actionDefine);
        }
        public bool getActionDefine(int cardPoolId, int id, out SerializableActionDefine actionDefine)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            actionDefine = cardPool.actionDefineList.Find(a => a.id == id);
            return actionDefine != null;
        }
        public SerializableActionDefine[] getActionDefines(int cardPoolId)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            return cardPool.actionDefineList.ToArray();
        }
    }
}
