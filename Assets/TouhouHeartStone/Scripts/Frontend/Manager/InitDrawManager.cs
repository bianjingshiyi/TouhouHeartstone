using UnityEngine;
using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend.Manager
{
    public class InitDrawManager : FrontendSubManager
    {
        CardFace[] initCards;

        [SerializeField]
        CardSelector selectorPrefab;

        /// <summary>
        /// 向初始抽卡管理器中添加几张卡
        /// </summary>
        /// <param name="card"></param>
        public void AddCards(CardFace[] cards)
        {
            initCards = cards;

            int count = cards.Length;
            var poses = GetInitialDrawPosition(count);

            for (int i = 0; i < count; i++)
            {
                if (i == count - 1)
                    cards[i].CardAniController.DrawCard(poses[i], onDrawcardAnimationFinish);
                else
                    cards[i].CardAniController.DrawCard(poses[i]);
            }
        }

        /// <summary>
        /// stage1: 抽卡动画播放完毕，播放抛硬币动画(划掉)，进入卡片选择模式
        /// </summary>
        void onDrawcardAnimationFinish()
        {
            foreach (var item in initCards)
            {
                Instantiate(selectorPrefab, item.transform);
            }
        }

        /// <summary>
        /// 确认选择这几张卡
        /// </summary>
        public void Confirm()
        {
            Debug.Log("OnClick.");
        }

        public Vector3[] GetInitialDrawPosition(int count)
        {
            Vector3 basePos = new Vector3(0, 0, -2);
            const float minSpacing = 2;
            const float maxWidth = 10;
            Vector3[] pos = new Vector3[count];

            float sp, offset;

            if (minSpacing * count > maxWidth)
            {
                sp = minSpacing;
                offset = -((count - 1) * minSpacing / 2);
            }
            else
            {
                sp = maxWidth / count;
                offset = sp / 2 - maxWidth / 2;
            }

            for (int i = 0; i < count; i++)
            {
                pos[i] = basePos;
                pos[i].x = offset + sp * i;
            }

            return pos;
        }

    }
}
