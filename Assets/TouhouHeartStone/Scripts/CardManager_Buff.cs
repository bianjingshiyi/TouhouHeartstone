using BJSYGameCore;
using TouhouCardEngine;
using System.Collections.Generic;
using UI;
using UnityEngine;
using System.Linq;
using TouhouCardEngine.Shared;
using Controllers;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public ICardPoolReference getCardPoolReferenceOfBuff(BuffDefine buff)
        {
            if (isBuiltinCardPool(buff.cardPoolId))
                return builtinCardPoolReference;
            foreach (var pair in _cardPoolDict)
            {
                if (pair.Key.CardPoolID == buff.cardPoolId && pair.Value.buffDefineList.Contains(buff))
                {
                    return pair.Key;
                }
            }
            return null;
        }
        public GeneratedBuffDefine createBuffDefine(long cardPoolId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
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
        public void addBuffDefine(long cardPoolId, GeneratedBuffDefine define)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            cardPool.buffDefineList.Add(define);
        }
        public GeneratedBuffDefine cloneBuffDefine(GeneratedBuffDefine buffDefine)
        {
            var json = new SerializableBuffDefine(buffDefine).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableBuffDefine>(json);
            var nodeDefiner = getNodeDefiner();
            var clone = cloneSeri.toGeneratedBuffDefine(nodeDefiner);
            clone.cardPoolId = buffDefine.cardPoolId;
            return clone;
        }
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
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            return cardPool.buffDefineList.Remove(buffDefine);
        }
        public bool tryGetBuffDefine(ICardPoolReference cardPoolRef, int buffDefineId, out GeneratedBuffDefine buffDefine)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolRef);
            }
            buffDefine = cardPool.buffDefineList.Find(b => b.id == buffDefineId);
            return buffDefine != null;
        }
        public Dictionary<int, BuffDefine> getBuffDefines(ICardPoolReference cardPoolRef)
        {
            if (!tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolRef);
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
        public BuffSkin createBuffSkin(long cardPoolId, int buffId, string language)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            BuffSkin buffSkin = new BuffSkin()
            {
                language = language,
                buffId = buffId
            };
            if (cardPool.buffSkinList.Find(s => s.buffId == buffId && s.language == language) != null)
            {
                Debug.LogWarning("已经存在增益" + buffId + "的语言为" + language + "的皮肤");
            }
            return buffSkin;
        }
        public void addBuffSkin(long cardPoolId, BuffSkin buffSkin)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            cardPool.buffSkinList.Add(buffSkin);
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
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
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
                throw new KeyNotFoundException("不存在卡池" + cardPoolRef);
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
                throw new KeyNotFoundException("不存在卡池" + cardPoolRef);
            }
            return cardPool.buffSkinList.ToArray();
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// 外部增益定义字段，Key为卡池Id，值为增益定义列表
        /// </summary>
        //Dictionary<int, List<GeneratedBuffDefine>> _externalBuffDefineDict = new Dictionary<int, List<GeneratedBuffDefine>>();
        //Dictionary<int, List<BuffSkin>> _externalBuffSkinDict = new Dictionary<int, List<BuffSkin>>();
        #endregion
    }
}
