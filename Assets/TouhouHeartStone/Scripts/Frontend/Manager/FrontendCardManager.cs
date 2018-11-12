using UnityEngine;
using System.Collections.Generic;
using System;


namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendCardManager : FrontendSubManager
    {
        HandCardManager hand => getSiblingManager<HandCardManager>();

        InitDrawManager initDraw => getSiblingManager<InitDrawManager>();

        [SerializeField]
        CardFace cardPrefab;

        [SerializeField]
        Transform cardSpawnRoot;

        protected new void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 按卡片类型生成一张卡
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        private CardFace SpawnCard(CardInstance card)
        {
            // todo: 根据卡片的类型ID来生成
            var instance = Instantiate(cardPrefab, cardSpawnRoot);
            instance.SetInstanceID(card.instanceId);
            instance.gameObject.SetActive(true);

            return instance;
        }

        #region 单抽

        CardFace singleDrawTemp;
        Queue<CardInstance> cardQueue = new Queue<CardInstance>();

        /// <summary>
        /// 单抽一张卡
        /// </summary>
        /// <param name="card"></param>
        public void NormalDrawCard(CardInstance card)
        {
            cardQueue.Enqueue(card);

            // 如果是第一张卡 就立即播放
            if (cardQueue.Count == 1)
                drawCardInner(card);
        }

        public void NormalDrawCard(CardInstance[] card)
        {
            foreach (var item in card)
            {
                NormalDrawCard(item);
            }
        }

        private void drawCardInner(CardInstance card)
        {
            singleDrawTemp = SpawnCard(card);
            singleDrawTemp.CardAniController.DrawCard(new Vector3(1, 0, -1), singleDrawFinish);
        }

        protected void singleDrawFinish()
        {
            // 动画播放完毕后，就加入到手牌
            cardQueue.Dequeue();
            hand.AddCard(singleDrawTemp);
            
            if(cardQueue.Count > 0)
            {
                drawCardInner(cardQueue.Peek());
            }
        }



        #endregion

        #region 多抽

        public void InitDrawCard(CardInstance[] cards)
        {
            CardFace[] instances = new CardFace[cards.Length];

            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                instances[i] = SpawnCard(card);
            }

            initDraw.AddCards(instances);
        }

        #endregion

        public void Start()
        {

        }
    }
}
