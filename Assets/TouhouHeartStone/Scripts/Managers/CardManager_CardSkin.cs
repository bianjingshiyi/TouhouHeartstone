using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using IGensoukyo.Utilities;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Game
{
    public partial class CardManager
    {
        #region 公有方法

        #region 创建卡牌皮肤
        public CardSkinData createSkin(long cardPoolId, int cardDefineId)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                CardSkinData skin = new()
                {
                    id = cardDefineId,
                    skinName = StringTable.EDITOR_NEW_SKIN_DEFAULT_NAME
                };

                var skinIds = getAllCardSkins(cardPoolRef, cardDefineId).GroupBy(c => c.skinId).Select(g => g.Key);
                if (skinIds.Count() > 0)
                    skin.skinId = skinIds.Max() + 1;
                else
                    skin.skinId = 1;
                return skin;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        public CardSkinData[] cloneCardSkins(ICardPoolReference poolRef, int cardId)
        {
            return cloneCardSkins(getAllCardSkins(poolRef, cardId));
        }
        public CardSkinData[] cloneCardSkins(CardSkinData[] skins)
        {
            return Array.ConvertAll(skins, s => new CardSkinData(s));
        }
        #endregion

        #region 添加卡牌皮肤
        public void addCardSkin(long cardPoolId, CardSkinData cardSkin)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.cardSkinList.Add(cardSkin);
        }
        #endregion

        #region 删除卡牌皮肤
        public void removeAllCardSkins(long cardPoolId, int cardDefineId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.cardSkinList.RemoveAll(s => s.id == cardDefineId);
        }
        public void removeSkin(long cardPoolId, int cardDefineId, int skinId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                cardPool.cardSkinList.RemoveAll(s => s.id == cardDefineId && s.skinId == skinId);
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        #endregion

        #region 获取卡牌皮肤
        public CardSkinData getDefaultSkin(CardDefine cardDefine)
        {
            if (cardDefine == null)
                return null;
            var poolRef = getCardPoolReferenceOfCard(cardDefine);
            return getDefaultSkin(poolRef, cardDefine.id);
        }
        public CardSkinData getDefaultSkin(ICardPoolReference cardPoolRef, int cardId)
        {
            if (tryGetDefaultCardSkin(cardPoolRef, cardId, out var skin))
            {
                return skin;
            }
            return null;
        }
        public bool tryGetDefaultCardSkin(ICardPoolReference cardPoolRef, int cardId, out CardSkinData skin)
        {
            if (cardPoolRef == null)
            {
                skin = null;
                return false;
            }
            var skins = getAllCardSkins(cardPoolRef, cardId);
            skin = skins.FirstOrDefault();
            return skin != null;
        }
        /// <summary>
        /// 根据id获取卡片皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardSkinData getCardSkin(ICardPoolReference cardPoolRef, int cardId) => getDefaultSkin(cardPoolRef, cardId);
        public CardSkinData getCardSkin(CardDefine cardDefine) => getDefaultSkin(cardDefine);
        public bool tryGetCardSkin(ICardPoolReference cardPoolRef, int cardId, out CardSkinData skin) => tryGetDefaultCardSkin(cardPoolRef, cardId, out skin);
        public CardSkinData[] getAllCardSkins(ICardPoolReference cardPoolRef, int cardId)
        {
            List<CardSkinData> skinList = new List<CardSkinData>();
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                skinList.AddRange(cardPool.cardSkinList.Where(s => s.id == cardId));
            }
            return skinList.ToArray();
        }
        #endregion

        #endregion
    }
}
