using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public GeneratedActionDefine createActionDefine(long cardPoolId, string name)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                var actDefList = cardPool.actionDefineList;
                //分配Id
                int id = 0;
                for (int i = 0; i < actDefList.Count; i++)
                {
                    if (actDefList[i].id > id)
                    {
                        id = actDefList[i].id;
                    }
                }
                id++;
                GeneratedActionDefine actionDefine = new GeneratedActionDefine(id, null, name, null, null, null, null, null);
                cardPool.actionDefineList.Add(actionDefine);

                return actionDefine;
            }
            return null;
        }
        public bool removeActionDefine(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                return cardPool.actionDefineList.Remove(actionDefine);
            }
            return false;
        }
        public bool getActionDefine(long cardPoolId, string name, out GeneratedActionDefine actionDefine)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                actionDefine = cardPool.actionDefineList.Find(g => g.defineName == name);
                return actionDefine != null;
            }
            actionDefine = null;
            return false;
        }
        public GeneratedActionDefine[] getActionDefines(long cardPoolId)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                return cardPool.actionDefineList.ToArray();
            }
            return new GeneratedActionDefine[0];
        }
        #endregion
    }
}
