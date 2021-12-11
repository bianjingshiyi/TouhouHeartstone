using BJSYGameCore;
using TouhouCardEngine;
using System.Collections.Generic;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public GeneratedBuffDefine createBuffDefine(int cardPoolId)
        {
            if (!_externalBuffDefineDict.TryGetValue(cardPoolId, out var buffDefineList))
            {
                buffDefineList = new List<GeneratedBuffDefine>();
                _externalBuffDefineDict.Add(cardPoolId, buffDefineList);
            }
            int newBuffDefineId = 0;
            for (int i = 0; i < buffDefineList.Count; i++)
            {
                if (buffDefineList[i].getId() > newBuffDefineId)
                    newBuffDefineId = buffDefineList[i].getId();
            }
            newBuffDefineId++;
            GeneratedBuffDefine buffDefine = new GeneratedBuffDefine(newBuffDefineId);
            buffDefineList.Add(buffDefine);
            return buffDefine;
        }
        public bool removeBuffDefine(int cardPoolId, GeneratedBuffDefine buffDefine)
        {
            if (_externalBuffDefineDict.TryGetValue(cardPoolId, out var buffDefineList))
            {
                return buffDefineList.Remove(buffDefine);
            }
            else
                return false;
        }
        public bool tryGetBuffDefine(int cardPoolId, int buffDefineId, out GeneratedBuffDefine buffDefine)
        {
            if (_externalBuffDefineDict.TryGetValue(cardPoolId, out var buffDefineList))
            {
                buffDefine = buffDefineList.Find(b => b.id == buffDefineId);
                return buffDefine != null;
            }
            else
            {
                buffDefine = null;
                return false;
            }
        }
        public GeneratedBuffDefine[] getBuffDefines(int cardPoolId)
        {
            if (_externalBuffDefineDict.TryGetValue(cardPoolId, out var buffList))
            {
                return buffList.ToArray();
            }
            else
                return new GeneratedBuffDefine[0];
        }
        public string[] getBuffDefineNames(BuffDefine[] buffDefines)
        {
            string[] buffDefineNames = new string[buffDefines.Length];
            for (int i = 0; i < buffDefines.Length; i++)
            {
                buffDefineNames[i] = buffDefines[i].ToString();
            }
            return buffDefineNames;
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// 外部增益定义字段，Key为卡池Id，值为增益定义列表
        /// </summary>
        Dictionary<int, List<GeneratedBuffDefine>> _externalBuffDefineDict = new Dictionary<int, List<GeneratedBuffDefine>>();
        #endregion
    }
}
