using System.Collections.Generic;
using System.Linq;
using Controllers;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        #region 增益定义

        #region 创建
        public GeneratedBuffDefine createBuffDefine(long cardPoolId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            int newBuffDefineId = 0;
            for (int i = 0; i < cardPool.buffDefineList.Count; i++)
            {
                if (cardPool.buffDefineList[i].id > newBuffDefineId)
                    newBuffDefineId = cardPool.buffDefineList[i].id;
            }
            newBuffDefineId++;
            GeneratedBuffDefine buffDefine = new GeneratedBuffDefine(newBuffDefineId)
            {
                cardPoolId = cardPoolId
            };
            return buffDefine;
        }
        public GeneratedBuffDefine cloneBuffDefine(GeneratedBuffDefine buffDefine)
        {
            var json = new SerializableBuffDefine(buffDefine).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableBuffDefine>(json);
            var nodeDefiner = getLocalNodeDefiner();
            var clone = cloneSeri.toGeneratedBuffDefine(nodeDefiner);
            clone.cardPoolId = buffDefine.cardPoolId;
            return clone;
        }
        #endregion

        #region 添加
        public void addBuffDefine(long cardPoolId, GeneratedBuffDefine define)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.buffDefineList.Add(define);
        }
        #endregion

        #region 删除
        public bool removeBuffDefine(long cardPoolId, int buffId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetBuffDefine(poolRef, buffId, out var buffDefine))
            {
                return removeBuffDefine(cardPoolId, buffDefine);
            }
            return false;
        }
        public bool removeBuffDefine(long cardPoolId, GeneratedBuffDefine buffDefine)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            return cardPool.buffDefineList.Remove(buffDefine);
        }
        #endregion

        #region 获取
        public bool tryGetBuffDefine(ICardPoolReference cardPoolRef, int buffDefineId, out GeneratedBuffDefine buffDefine)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolRef}");
            }
            buffDefine = cardPool.buffDefineList.Find(b => b.id == buffDefineId);
            return buffDefine != null;
        }
        public Dictionary<int, BuffDefine> getBuffDefines(ICardPoolReference cardPoolRef)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolRef}");
            }
            return cardPool.buffDefineList.ToDictionary(b => b.id, b => b as BuffDefine);
        }
        public Dictionary<CardPoolReference, Dictionary<int, BuffDefine>> getBuffDefines(IEnumerable<CardPoolReference> cardPoolsRef)
        {
            Dictionary<CardPoolReference, Dictionary<int, BuffDefine>> buffDefDict = new Dictionary<CardPoolReference, Dictionary<int, BuffDefine>>();
            foreach (var cardPoolRef in cardPoolsRef)
            {
                buffDefDict.Add(cardPoolRef, getBuffDefines(cardPoolRef));
            }
            return buffDefDict;
        }
        #endregion

        #endregion

        #region 增益皮肤

        #region 创建
        public BuffSkin createBuffSkin(long cardPoolId, int buffId, string language)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            BuffSkin buffSkin = new BuffSkin()
            {
                language = language,
                buffId = buffId
            };
            if (cardPool.buffSkinList.Find(s => s.buffId == buffId && s.language == language) != null)
            {
                Debug.LogWarning($"已经存在增益{buffId}的语言为{language}的皮肤");
            }
            return buffSkin;
        }
        public BuffSkin[] cloneBuffSkins(long cardPoolId, int buffId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            var skins = getBuffSkins(poolRef, buffId);
            return cloneBuffSkins(skins);
        }
        public BuffSkin[] cloneBuffSkins(CardPoolInfo info, int buffId)
        {
            return cloneBuffSkins(getBuffSkins(info.ToReference(), buffId));
        }
        public BuffSkin[] cloneBuffSkins(BuffSkin[] skins)
        {
            var json = skins.toJson();
            var clone = JsonHelper.fromJson<BuffSkin[]>(json);
            return clone;
        }
        #endregion

        #region 添加
        public void addBuffSkin(long cardPoolId, BuffSkin buffSkin)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.buffSkinList.Add(buffSkin);
        }
        #endregion

        #region 删除
        public void removeAllBuffSkins(long cardPoolId, int buffId)
        {
            var pool = getLocalCardPool(cardPoolId);
            if (pool != null)
            {
                pool.buffSkinList.RemoveAll(s => s.buffId == buffId);
            }
        }
        public bool removeBuffSkin(BuffSkin buffSkin)
        {
            foreach (var cardPool in _cardPoolDict.Values)
            {
                if (cardPool.buffSkinList.Remove(buffSkin))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 获取
        /// <summary>
        /// 获取增益定义的皮肤，有当前语言优先获取当前语言，没有就获取第一个，再没有就返回空
        /// </summary>
        /// <param name="buffDefine"></param>
        /// <returns></returns>
        public BuffSkin getBuffSkin(BuffDefine buffDefine)
        {
            long cardPoolId = buffDefine.cardPoolId;
            if (!tryGetCardPool(getCardPoolReferenceOfBuff(buffDefine), out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            var buffSkinList = cardPool.buffSkinList.FindAll(s => s.buffId == buffDefine.id);
            BuffSkin buffSkin = buffSkinList.Find(s => s.language == game.langManager.getLanguage());
            if (buffSkin == null)
                buffSkin = buffSkinList.Count > 0 ? buffSkinList[0] : null;
            return buffSkin;
        }
        public BuffSkin[] getBuffSkins(ICardPoolReference cardPoolRef, int buffId)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolRef}");
            }
            List<BuffSkin> buffSkinList = new List<BuffSkin>();
            for (int i = 0; i < cardPool.buffSkinList.Count; i++)
            {
                if (cardPool.buffSkinList[i].buffId == buffId)
                {
                    buffSkinList.Add(cardPool.buffSkinList[i]);
                }
            }
            return buffSkinList.ToArray();
        }
        public BuffSkin[] getBuffSkins(ICardPoolReference cardPoolRef)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolRef}");
            }
            return cardPool.buffSkinList.ToArray();
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
        #endregion

        #endregion

        #endregion
    }
}
