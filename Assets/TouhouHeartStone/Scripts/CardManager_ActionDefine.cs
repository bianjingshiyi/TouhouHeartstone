using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public GeneratedActionDefine createActionDefine(long cardPoolId, string name)
        {
            if (!_runtimeGenActDefDict.TryGetValue(cardPoolId, out var actDefList))
            {
                actDefList = new List<GeneratedActionDefine>();
                _runtimeGenActDefDict.Add(cardPoolId, actDefList);
            }
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
            _runtimeGenActDefDict[cardPoolId].Add(actionDefine);
            return actionDefine;
        }
        public bool removeActionDefine(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            if (_runtimeGenActDefDict.TryGetValue(cardPoolId, out var actDefList))
            {
                return actDefList.Remove(actionDefine);
            }
            return false;
        }
        public bool getActionDefine(long cardPoolId, string name, out GeneratedActionDefine actionDefine)
        {
            if (_runtimeGenActDefDict.TryGetValue(cardPoolId, out var actDefList))
            {
                actionDefine = actDefList.Find(g => g.defineName == name);
                return actionDefine != null;
            }
            actionDefine = null;
            return false;
        }
        public GeneratedActionDefine[] getActionDefines(long cardPoolId)
        {
            if (_runtimeGenActDefDict.TryGetValue(cardPoolId, out var actDefList))
            {
                return actDefList.ToArray();
            }
            return new GeneratedActionDefine[0];
        }
        #endregion
        #region 属性字段
        Dictionary<long, List<GeneratedActionDefine>> _runtimeGenActDefDict = new Dictionary<long, List<GeneratedActionDefine>>();
        #endregion
    }
}
