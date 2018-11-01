using UnityEngine;

using System.Collections.Generic;
using System;

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
            var pos = getCardPosInfo(cards.Count + 1, origCnt);
            addCardInner(card, pos);
            adjustCardPos(origCnt);
        }

        /// <summary>
        /// 向手牌中添加N张卡
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(CardFace[] card)
        {
            int origCnt = cards.Count;

            for (int i = 0; i < card.Length; i++)
            {
                var pos = getCardPosInfo(origCnt + card.Length, origCnt + i);
                addCardInner(card[i], pos);
            }

            adjustCardPos(origCnt);
        }

        private void addCardInner(CardFace card, CardPosInfo pos)
        {
            cards.Add(card);
            card.OnMouseIn += cardOnMouseIn;
            card.OnMouseOut += cardOnMouseOut;
            card.CardAniController.CardMoveToHand(pos.Position, pos.Rotation);
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


            if (count <= 3)
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


        private void cardOnMouseOut(CardFace obj)
        {
            if(obj.State == CardState.Active)
            {
                var p = getCardPosInfo(cards.Count, cards.IndexOf(obj));
                obj.CardAniController.UnShowCard(p.Position, p.Rotation);
            }
        }

        private void cardOnMouseIn(CardFace obj)
        {
            if (obj.State == CardState.Hand)
            {
                var p = getCardPosInfo(cards.Count, cards.IndexOf(obj));

                p.Position *= 0.75f;

                obj.CardAniController.ShowCard(p.Position);
            }
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
