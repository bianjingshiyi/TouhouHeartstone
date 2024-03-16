using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
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
            GeneratedBuffDefine buffDefine = new GeneratedBuffDefine(cardPoolId, newBuffDefineId);
            return buffDefine;
        }
        public GeneratedBuffDefine cloneBuffDefine(GeneratedBuffDefine buffDefine)
        {
            var json = new SerializableBuffDefine(buffDefine).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableBuffDefine>(json);
            var nodeDefiner = getLocalNodeDefiner();
            var clone = cloneSeri.toGeneratedBuffDefine(nodeDefiner);
            clone.cardPoolId = buffDefine.cardPoolId;
            foreach (var effect in clone.effectList)
            {
                effect.buffDefineRef = clone.getDefineRef();
                effect.cardDefineRef = null;
            }
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
            if (cardPool.buffDefineList.Remove(buffDefine))
            {
                cardPool.buffSkinList.RemoveAll(skin => skin.buffId == buffDefine.id);
                return true;
            }
            return false;
        }
        #endregion

        #region 获取
        public bool tryGetBuffDefine(ICardPoolReference cardPoolRef, int buffDefineId, out GeneratedBuffDefine buffDefine)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                buffDefine = null;
                return false;
            }
            buffDefine = cardPool.buffDefineList.Find(b => b.id == buffDefineId);
            return buffDefine != null;
        }
        public Dictionary<int, BuffDefine> getBuffDefines(ICardPoolReference cardPoolRef)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return null;
            }
            return cardPool.buffDefineList.ToDictionary(b => b.id, b => b as BuffDefine);
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
        public BuffSkin[] cloneLocalBuffSkins(long cardPoolId, int buffId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return cloneBuffSkins(poolRef, buffId);
        }
        public BuffSkin[] cloneBuffSkins(BuffDefine buff)
        {
            var skins = getBuffSkins(buff);
            return cloneBuffSkins(skins);
        }
        public BuffSkin[] cloneBuffSkins(ICardPoolReference poolRef, int buffId)
        {
            var skins = getBuffSkins(poolRef, buffId);
            return cloneBuffSkins(skins);
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
            foreach (var cardPool in _localCardPoolDict.Values)
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
        public BuffSkin getDefaultBuffSkin(BuffDefine buffDefine)
        {
            var poolRef = getCardPoolReferenceOfBuff(buffDefine);
            return getDefaultBuffSkin(poolRef, buffDefine.id);
        }
        /// <summary>
        /// 获取增益定义的皮肤，有当前语言优先获取当前语言，没有就获取第一个，再没有就返回空
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <param name="buffId"></param>
        /// <returns></returns>
        public BuffSkin getDefaultBuffSkin(ICardPoolReference cardPoolRef, int buffId)
        {
            var buffSkinList = getBuffSkins(cardPoolRef, buffId);
            return buffSkinList.FirstOrDefault(s => s.language == game.langManager.getLanguage()) ?? buffSkinList.FirstOrDefault();
        }
        public BuffSkin getBuffSkin(BuffDefine buffDefine) => getDefaultBuffSkin(buffDefine);
        public BuffSkin getBuffSkin(ICardPoolReference cardPoolRef, int buffId) => getDefaultBuffSkin(cardPoolRef, buffId);
        /// <summary>
        /// 获取增益定义的皮肤，有当前语言优先获取当前语言，没有就获取第一个，再没有就返回空
        /// </summary>
        /// <param name="buffDefine"></param>
        /// <returns></returns>
        public BuffSkin[] getBuffSkins(BuffDefine buffDefine)
        {
            var poolRef = getCardPoolReferenceOfBuff(buffDefine);
            return getBuffSkins(poolRef, buffDefine.id);
        }
        public BuffSkin[] getBuffSkins(ICardPoolReference cardPoolRef, int buffId)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return Array.Empty<BuffSkin>();
            }
            return cardPool.buffSkinList.Where(s => s.buffId == buffId).ToArray();
        }
        public string[] getBuffDefineNames(BuffDefine[] buffDefines)
        {
            return buffDefines.Select(b => getDefaultBuffSkin(b)?.name ?? b.ToString()).ToArray();
        }
        #endregion

        #endregion

        #endregion
    }
}
