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

        public void AddCard(CardFace[] card)
        {
            int origCnt = cards.Count;
            cards.AddRange(card);

            for (int i = 0; i < card.Length; i++)
            {
                var pos = getCardPosInfo(cards.Count, i + origCnt);
                card[i].CardAniController.CardMoveToHand(pos.Position, pos.Rotation);
            }

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
            return new CardPosInfo() { Position = new Vector3(0, -1.11f, -0.1f) };
        }

        /// <summary>
        /// 计算第n张卡的位置信息
        /// </summary>
        /// <param name="count">卡片数目</param>
        /// <param name="n">第x张（0开始）</param>
        /// <returns></returns>
        CardPosInfo getCardPosInfo(int count, int n)
        {
            var basePos = new Vector3(0, -1.11f, -0.1f);
            const float minSpacing = 0.5f;
            const float maxWidth = 2;
            if(count <= 3)
            {
                var offset = minSpacing * (count - 1) / 2;
                basePos.x = minSpacing * n - offset;

                return new CardPosInfo() { Position = basePos, Rotation = Vector3.zero };
            }
            else
            {
                var step = maxWidth / (count - 1);
                basePos.x = step * n - maxWidth / 2;
                basePos.z -= 0.1f * n;

                var rot = new Vector3();
                rot.z = (n - (count / 2f)) * 15;
                return new CardPosInfo() { Position = basePos, Rotation = rot };
            }
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
