using UnityEngine;

using System.Collections.Generic;


namespace TouhouHeartstone.Frontend.Manager
{
    public class HandCardManager : FrontendSubManager
    {
        List<CardFace> cards = new List<CardFace>();

        /// <summary>
        /// 向手牌中添加一张卡
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(CardFace card)
        {
            cards.Add(card);

            var pos = getNextCardInfo();
            card.CardAniController.CardMoveToHand(pos.Position, pos.Rotation);

            adjustCardPos(true);
        }

        struct CardPosInfo
        {
            public Vector3 Position;
            public Vector3 Rotation;
        }

        CardPosInfo getNextCardInfo()
        {
            // todo: 计算真正的卡片信息
            return new CardPosInfo();
        }

        /// <summary>
        /// 调整卡片位置
        /// </summary>
        /// <param name="ignoreLast">是否忽略最新加入的那张牌</param>
        void adjustCardPos(bool ignoreLast)
        {

        }
    }
}
