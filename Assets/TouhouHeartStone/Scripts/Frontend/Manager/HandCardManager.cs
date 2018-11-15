using UnityEngine;

using System.Collections.Generic;
using System;

namespace TouhouHeartstone.Frontend.Manager
{
    public class HandCardManager : FrontendSubManager
    {
        List<CardFace> cards = new List<CardFace>();

        [SerializeField]
        Transform selfImage;

        bool isCardInHand
        {
            get
            {
                var mPos = Input.mousePosition;
                var tPos = selfImage.position;
                var cam = Camera.main;

                mPos.z = cam.transform.position.y - tPos.y;
                var wPos = cam.ScreenToWorldPoint(mPos);

                return tPos.z > wPos.z;
            }
        }

        DeskEntityManager desk => getSiblingManager<DeskEntityManager>();

        TargetSelector selector => getSiblingManager<FrontendUIManager>().TargetSelector;

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
            cardRegistEvent(card);
            card.SetHand(this);
            card.CardAniController.CardMoveToHand(pos.Position, pos.Rotation);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="card"></param>
        private void cardRegistEvent(CardFace card)
        {
            card.OnMouseIn += cardOnMouseIn;
            card.OnMouseOut += cardOnMouseOut;
            card.OnClick += cardOnClick;
            card.OnDrag += cardOnDrag;
            card.OnRelease += cardOnRelease;
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

        /// <summary>
        /// 鼠标移出事件
        /// 主要用于处理详细信息的显示切换
        /// </summary>
        /// <param name="obj"></param>
        private void cardOnMouseOut(CardFace obj)
        {
            if (activeCard != null) return;
            if (obj.State == CardState.Active)
            {
                var p = getCardPosInfo(cards.Count, cards.IndexOf(obj));
                obj.CardAniController.UnShowCard(p.Position, p.Rotation);
            }
        }

        /// <summary>
        /// 鼠标移入事件
        /// 主要用于处理详细信息的显示切换
        /// </summary>
        /// <param name="obj"></param>
        private void cardOnMouseIn(CardFace obj)
        {
            if (activeCard != null) return;
            if (obj.State == CardState.Hand)
            {
                var p = getCardPosInfo(cards.Count, cards.IndexOf(obj));
                p.Position *= 0.75f;
                obj.CardAniController.ShowCard(p.Position);
            }
        }

        /// <summary>
        /// 卡片点击事件
        /// 用于处理点击拿起和使用
        /// </summary>
        /// <param name="card"></param>
        private void cardOnClick(CardFace card)
        {
            switch (card.State)
            {
                case CardState.Pickup:
                    cardOnRelease(card);
                    break;
                case CardState.Active:
                    cardOnDrag(card);
                    break;
            }
        }

        /// <summary>
        /// 卡片拖动事件
        /// 用于使用卡片
        /// </summary>
        /// <param name="card"></param>
        private void cardOnDrag(CardFace card)
        {
            if (card.State == CardState.Active)
            {
                activeCard = card;
                card.State = CardState.Pickup;

                card.CardAniController.StopAll();

                // 设置拿起后的位置
                var p = getCardPosInfo(cards.Count, cards.IndexOf(card));
                p.Position.z -= 0.05f;
                card.transform.localPosition = p.Position;
                card.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }

        /// <summary>
        /// 卡片被释放的事件
        /// </summary>
        /// <param name="card"></param>
        private void cardOnRelease(CardFace card)
        {
            if (card.State == CardState.Pickup)
            {
                // 根据卡的类型来做
                switch (card.Type)
                {
                    // 无目标法术卡，直接使用
                    case CardType.SpellDriftless:
                        UseSpellDriftlessCard(card);
                        break;
                    // 无目标实体卡，加入到位置中
                    case CardType.EntityDriftless:
                        UseEntityDriftlessCard(card);
                        break;
                    // 有目标法术卡，使用目标选择器进行选择
                    case CardType.SpellDirected:
                        UseSpellDirectedCard(card);
                        break;
                    // 有目标实体卡：加入位置，然后使用目标选择器
                    case CardType.EntityDirected:
                        UseEntityDirectedCard(card);
                        break;
                }
            }
        }

        /// <summary>
        /// 有目标实体卡的使用
        /// </summary>
        /// <param name="card"></param>
        private void UseEntityDirectedCard(CardFace card)
        {
            if(stage2)
            {
                if(!selector.IsSelected || isCardInHand)
                {
                    desk.RemoveEntityByInstanceID(card.InstanceID);
                    card.AnimateIn();
                }
                else
                {
                    card.SetTarget(selector.GetLastSelectID());
                    useCard(card);
                }
                selector.HideSelector();
                stage2 = false;
            }
            else
            {
                if(isCardInHand)
                {
                    cardBackHand(card);
                    activeCard = null;
                    return;
                }

                var index = desk.ReserveInsertSpace(Input.mousePosition);
                desk.Insert(card, index);

                card.SetPosition(index);
                stage2 = true;
            }
        }

        /// <summary>
        /// 有目标法术卡的使用
        /// </summary>
        /// <param name="card"></param>
        private void UseSpellDirectedCard(CardFace card)
        {
            if (selector.LastSelectTarget.transform == null || isCardInHand)
            {
                cardBackHand(card);
                activeCard = null;
            }
            else
            {
                card.SetTarget(selector.GetLastSelectID());
                useCard(card);
            }
            selector.HideSelector();
        }

        /// <summary>
        /// 无目标实体卡的使用
        /// </summary>
        /// <param name="card"></param>
        private void UseEntityDriftlessCard(CardFace card)
        {
            // 检测是否在安全界限内
            if (isCardInHand)
            {
                cardBackHand(card);
                activeCard = null;
                return;
            }
            var p = desk.ReserveInsertSpace(Input.mousePosition);
            desk.Insert(card, p);
            useCard(card);
        }

        /// <summary>
        /// 无目标法术卡的使用
        /// </summary>
        /// <param name="card"></param>
        private void UseSpellDriftlessCard(CardFace card)
        {
            // 检测是否在安全界限内
            if (isCardInHand)
            {
                cardBackHand(card);
                activeCard = null;
                return;
            }
            useCard(card);
        }

        CardFace activeCard;
        bool stage2 = false;

        private void Update()
        {
            if (activeCard != null)
            {
                switch (activeCard.Type)
                {
                    // 无目标实体卡，跟随鼠标，同时在桌面上预留实体插入空间
                    case CardType.EntityDriftless:
                        cardFollowMouse();
                        if (isCardInHand) desk.UpdateEntityPos();
                        else desk.ReserveInsertSpace(Input.mousePosition);
                        break;
                    // 有目标实体卡，第一阶段与上面相同
                    case CardType.EntityDirected:
                        if (!stage2)
                        {
                            cardFollowMouse();
                            if (isCardInHand) desk.UpdateEntityPos();
                            else desk.ReserveInsertSpace(Input.mousePosition);
                        }
                        else
                        {
                            selector.SetStartPosition(desk.GetPositionByIndex(desk.GetIndexByCardID(activeCard.InstanceID)));
                            // todo: 使用真正的instance逻辑
                            selector.UpdatePos(Input.mousePosition, (obj) => { return true; });

                            // 生成事件。用于stage2的按下检测
                            if (Input.GetMouseButtonDown(0))
                            {
                                cardOnRelease(activeCard);
                            }
                        }
                        break;
                    // 无目标法术卡，跟随鼠标
                    case CardType.SpellDriftless:
                        cardFollowMouse();
                        break;
                    // 有目标法术卡，在界限内使用特殊方法跟随鼠标；在界限外使用目标选择器选择目标
                    case CardType.SpellDirected:
                        cardSpecialFollowMouse();
                        if (!isCardInHand)
                        {
                            selector.SetStartPosition(selfImage.position);
                            selector.UpdatePos(Input.mousePosition, (obj) => { return true; });
                        }
                        else
                        {
                            selector.HideSelector();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 卡片跟随鼠标
        /// </summary>
        private void cardFollowMouse()
        {
            var t = activeCard.gameObject.transform;
            var orgpos = t.position;

            var mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.y - t.position.y;
            var world = Camera.main.ScreenToWorldPoint(mousePos);

            orgpos.x = world.x;
            orgpos.z = world.z;

            t.position = orgpos;
        }

        /// <summary>
        /// 特殊的跟随：会逐渐进入本人位置
        /// </summary>
        private void cardSpecialFollowMouse()
        {
            var cardTransform = activeCard.gameObject.transform;
            var cardOrigPos = cardTransform.position;
            var cam = Camera.main;
            var mousePos = Input.mousePosition;
            var targetPos = selfImage.position;

            const float distFactor = 1f;

            mousePos.z = cam.transform.position.y - cardTransform.position.y;
            var cardWorldPos = cam.ScreenToWorldPoint(mousePos);

            var dist = targetPos.z - cardWorldPos.z;
            dist = dist > 0 ? dist : 0;

            Vector2 target = Vector2.zero;

            // 距离过远，使用传统的设置
            if (dist > distFactor)
            {
                target = MathUtils.xzy2xy(cardWorldPos);
            }
            else
            {
                target = Vector2.Lerp(MathUtils.xzy2xy(targetPos), MathUtils.xzy2xy(cardWorldPos), dist / distFactor);
            }

            cardOrigPos.x = target.x;
            cardOrigPos.z = target.y;

            cardTransform.position = cardOrigPos;
        }

        /// <summary>
        /// 销毁卡片
        /// </summary>
        /// <param name="card"></param>
        void cardDestroy(CardFace card)
        {
            card.State = CardState.Destory;
            if (cards.Contains(card)) cards.Remove(card);

            adjustCardPos(cards.Count);
        }

        /// <summary>
        /// 使用卡片
        /// </summary>
        /// <param name="card"></param>
        /// <param name="arg"></param>
        private void useCard(CardFace card)
        {
            card.Use();
            cardDestroy(card);
            activeCard = null;
        }

        /// <summary>
        /// 卡牌回到手牌
        /// </summary>
        /// <param name="card"></param>
        private void cardBackHand(CardFace card)
        {
            var p = getCardPosInfo(cards.Count, cards.IndexOf(card));
            card.CardAniController.UnShowCard(p.Position, p.Rotation);
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

        /// <summary>
        /// 使用某张卡
        /// </summary>
        /// <param name="cardID">卡片ID</param>
        /// <param name="position">实体出生位置。没有则设为 -1</param>
        /// <param name="targetID">目标物体。没有则设为-1，敌方本体设为0</param>
        public void UseCard(int cardID, int position = -1, int targetID = -1)
        {
            getSiblingManager<FrontendWitnessEventDispatcher>().InvokeUseCardEvent(cardID, position, targetID);
        }
    }
}
