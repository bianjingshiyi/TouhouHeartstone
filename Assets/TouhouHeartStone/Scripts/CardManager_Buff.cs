using BJSYGameCore;
using TouhouCardEngine;
using System.Collections.Generic;
using UI;
using UnityEngine;

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
        public GeneratedBuffDefine[] getBuffDefines(IEnumerable<int> cardPoolsId)
        {
            List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
            foreach (var id in cardPoolsId)
            {
                buffDefineList.AddRange(getBuffDefines(id));
            }
            return buffDefineList.ToArray();
        }
        public string[] getBuffDefineNames(BuffDefine[] buffDefines)
        {
            string[] buffDefineNames = new string[buffDefines.Length];
            for (int i = 0; i < buffDefines.Length; i++)
            {
                BuffSkin buffSkin = getBuffSkin(buffDefines[i]);
                buffDefineNames[i] = buffSkin != null ? buffSkin.name : buffDefines[i].ToString();
            }
            return buffDefineNames;
        }
        public BuffSkin createBuffSkin(int cardPoolId, int buffId, string language)
        {
            if (!_externalBuffSkinDict.TryGetValue(cardPoolId, out List<BuffSkin> buffSkinList))
            {
                buffSkinList = new List<BuffSkin>();
                _externalBuffSkinDict.Add(cardPoolId, buffSkinList);
            }
            BuffSkin buffSkin = new BuffSkin()
            {
                language = language,
                buffId = buffId
            };
            if (buffSkinList.Find(s => s.buffId == buffId && s.language == language) != null)
            {
                Debug.LogWarning("已经存在增益" + buffId + "的语言为" + language + "的皮肤");
            }
            buffSkinList.Add(buffSkin);
            return buffSkin;
        }
        public bool removeBuffSkin(BuffSkin buffSkin)
        {
            foreach (var pair in _externalBuffSkinDict)
            {
                if (pair.Value.Contains(buffSkin))
                {
                    pair.Value.Remove(buffSkin);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取增益定义的皮肤，有当前语言优先获取当前语言，没有就获取第一个，再没有就返回空
        /// </summary>
        /// <param name="buffDefine"></param>
        /// <returns></returns>
        public BuffSkin getBuffSkin(BuffDefine buffDefine)
        {
            int cardPoolId = getCardPoolIdOfBuffDefine(buffDefine);
            var buffSkinList = _externalBuffSkinDict[cardPoolId].FindAll(s => s.buffId == buffDefine.getId());
            BuffSkin buffSkin = buffSkinList.Find(s => s.language == game.langManager.getLanguage());
            if (buffSkin == null)
                buffSkin = buffSkinList.Count > 0 ? buffSkinList[0] : null;
            return buffSkin;
        }
        public int getCardPoolIdOfBuffDefine(BuffDefine buffDefine)
        {
            foreach (var pair in _externalBuffDefineDict)
            {
                if (pair.Value.Contains(buffDefine as GeneratedBuffDefine))
                {
                    return pair.Key;
                }
            }
            return 0;
        }
        public BuffSkin[] getBuffSkins(int cardPoolId, int buffId)
        {
            if (_externalBuffSkinDict.TryGetValue(cardPoolId, out var externalBuffSkinList))
            {
                List<BuffSkin> buffSkinList = new List<BuffSkin>();
                for (int i = 0; i < externalBuffSkinList.Count; i++)
                {
                    if (externalBuffSkinList[i].buffId == buffId)
                    {
                        buffSkinList.Add(externalBuffSkinList[i]);
                    }
                }
                return buffSkinList.ToArray();
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        public BuffSkin[] getBuffSkins(int cardPoolId)
        {
            if (_externalBuffSkinDict.TryGetValue(cardPoolId, out List<BuffSkin> buffSkinList))
            {
                return buffSkinList.ToArray();
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// 外部增益定义字段，Key为卡池Id，值为增益定义列表
        /// </summary>
        Dictionary<int, List<GeneratedBuffDefine>> _externalBuffDefineDict = new Dictionary<int, List<GeneratedBuffDefine>>();
        Dictionary<int, List<BuffSkin>> _externalBuffSkinDict = new Dictionary<int, List<BuffSkin>>();
        #endregion
    }
}
