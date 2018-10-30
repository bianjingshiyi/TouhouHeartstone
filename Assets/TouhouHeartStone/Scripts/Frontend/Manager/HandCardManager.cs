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
            int origCnt = cards.Count;
            cards.Add(card);

            var pos = getCardPosInfo(cards.Count, origCnt);
            card.CardAniController.CardMoveToHand(pos.Position, pos.Rotation);

            adjustCardPos(origCnt);
        }

        /// <summary>
        /// 向手牌中添加N张卡
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(CardFace[] card)
        {
            int origCnt = cards.Count;
            cards.AddRange(card);

            for (int i = 0; i < card.Length; i++)
            {
                var pos = getCardPosInfo(cards.Count, i + origCnt);
                card[i].CardAniController.CardMoveToHand(pos.Position, pos.Rotation);
            }

            adjustCardPos(origCnt);
        }

        struct CardPosInfo
        {
            public Vector3 Position;
            public Vector3 Rotation;
        }

        /// <summary>
        /// 计算第n张卡的位置信息
        /// </summary>
        /// <param name="count">卡片数目</param>
        /// <param name="n">第x张（0开始）</param>
        /// <returns></returns>
        CardPosInfo getCardPosInfo(int count, int n)
        {
            var basePos = new Vector3(0, -1.4f, -0.1f);
            const float normapSpacing = 0.5f;
            const float maxWidth = 1.8f;
            const float cardHalfHeight = 0.36f;


            if(count <= 3)
            {
                var offset = normapSpacing * (count - 1) / 2;
                basePos.x = normapSpacing * n - offset;

                return new CardPosInfo() { Position = basePos, Rotation = Vector3.zero };
            }
            else
            {
                var width = maxWidth + normapSpacing * 0.25f * (count - 3);
                var bt = (count - 1);
                var step = width / bt;
                var deg = ((bt / 2f) - n) * 10;

                basePos.x = step * n - width / 2 + cardHalfHeight * Mathf.Sin(deg * Mathf.Deg2Rad);
                basePos.y -= cardHalfHeight * (1 - Mathf.Cos(deg * Mathf.Deg2Rad)) * 3;
                basePos.z -= 0.02f * n;

                var rot = new Vector3();
                rot.z = deg;
                return new CardPosInfo() { Position = basePos, Rotation = rot };
            }
        }

        public void ShowCard()
        {

        }

        /// <summary>
        /// 调整卡片位置
        /// </summary>
        /// <param name="ignoreLast">牌的数目</param>
        void adjustCardPos(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = getCardPosInfo(cards.Count, i);
                cards[i].CardAniController.TweakPosition(pos.Position, pos.Rotation);
            }
        }
    }
}
