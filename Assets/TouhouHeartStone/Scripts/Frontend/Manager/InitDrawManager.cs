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

        [SerializeField]
        InitDrawUI ui;

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
            // 开启背板
            ui.gameObject.SetActive(true);

            // 设置选择器
            foreach (var item in initCards)
            {
                var cc = Instantiate(selectorPrefab, item.transform);
                cc.OnCardSelectStateChange += onCardSelectStateChange;
            }
        }

        /// <summary>
        /// 要移出卡片的列表
        /// </summary>
        List<int> removeList = new List<int>();

        void onCardSelectStateChange(CardFace card, bool state)
        {
            if (state)
            {
                removeList.Add(card.InstanceID);
            }
            else
            {
                if (removeList.Contains(card.InstanceID))
                    removeList.Remove(card.InstanceID);
            }
        }

        /// <summary>
        /// 确认选择这几张卡
        /// </summary>
        public void Confirm()
        {
            List<CardFace> cf = new List<CardFace>();

            // 移出不需要的卡
            for (int i = 0; i < initCards.Length; i++)
            {
                var item = initCards[i];

                if (removeList.Contains(item.InstanceID))
                {
                    // 移除卡片
                    Destroy(item.gameObject);
                    initCards[i] = null;
                }
                else
                {
                    // 移除卡片选择器
                    Destroy(item.GetComponentInChildren<CardSelector>().gameObject);
                    cf.Add(item);
                }
            }

            // 通知后端
            getSiblingManager<FrontendWitnessEventDispatcher>().ReplaceInitDrawAction?.Invoke(removeList.ToArray());

            // 关闭背板
            ui.gameObject.SetActive(false);

            getSiblingManager<HandCardManager>().AddCard(cf.ToArray());

            DebugUtils.LogDebug("OnClick.");
        }

        public Vector3[] GetInitialDrawPosition(int count)
        {
            Vector3 basePos = new Vector3(0, 0, -1);
            const float minSpacing = 0.5f;
            const float maxWidth = 4;
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
