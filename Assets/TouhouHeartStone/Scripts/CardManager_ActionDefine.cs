using System.Collections.Generic;
using System.Linq;
using Controllers;
using TouhouCardEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public GeneratedActionDefine createActionDefine(long cardPoolId, string name)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
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
                id++;
                var graph = new ActionGraph();
                GeneratedActionDefine actionDefine = new GeneratedActionDefine(graph, id, null, name, null, null, null);
                actionDefine.InitNodes();
                cardPool.actionDefineList.Add(actionDefine);

                return actionDefine;
            }
            return null;
        }
        public bool removeActionDefine(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.Remove(actionDefine);
            }
            return false;
        }
        public bool getActionDefine(long cardPoolId, string name, out GeneratedActionDefine actionDefine)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                actionDefine = cardPool.actionDefineList.Find(g => g.defineName == name);
                return actionDefine != null;
            }
            actionDefine = null;
            return false;
        }
        public GeneratedActionDefine getActionDefine(long cardPoolId, int id)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.Find(g => g.id == id);
            }
            return null;
        }
        public GeneratedActionDefine[] getActionDefines(long cardPoolId)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.actionDefineList.ToArray();
            }
            return new GeneratedActionDefine[0];
        }
        #endregion
    }
}
