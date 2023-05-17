using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UObject = UnityEngine.Object;
using Card = UI.Card;

namespace Game
{
    using Arg = HearthStoneRule.RevealEventArg;
    class RevealAnim : EventAnimation<Arg>
    {
        private CardSnapshot _card;
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            _card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);
            var player = arg.player;
            var shouldAnimate = arg.shouldAnimate;

            if (!shouldAnimate || _card == null)
            {
                yield break;
            }
            //在对应的卡组上创建卡牌实例

            // 获取卡牌源位置。
            var targetTransform = getCardTargetTransfrom(table, record, arg);
            var startHeight = getCardStartHeight(table, record, arg);

            // 如果该牌在你的手牌里，起始正面朝上。
            bool startFaceUp = player == table.player && _card.pile.name == PILE.HAND;
            Card cardUI = table.createInvokeCard(_card, targetTransform?.position ?? Vector3.zero, -1, startFaceUp);


            var startScale = Vector3.one * startHeight / cardUI.rectTransform.rect.height;
            cardUI.transform.localScale = startScale;

            if (table.tryGetHand(_card, out var hand))
            {
                hand.alpha = 0;
            }

            //播放位移曲线动画到对应的位置
            RectTransform targetRect = table.ui.LeftDisplayImage.rectTransform;
            TableAnimation animMove = new CardTransAnim(cardUI, targetRect, Vector3.one * targetRect.rect.height / cardUI.rectTransform.rect.height, table.cardShowCurve);
            while (!animMove.update(table, _time))
            {
                yield return null;
            }


            //翻转揭示
            if (!startFaceUp)
            {
                AnimAnim animFlipFace = new AnimAnim(cardUI.animator, cardUI._flipToFaceAnimName);
                table.setCard(cardUI, _card);
                while (!animFlipFace.update(table, _time))
                {
                    yield return null;
                }

            }

            // 等待1秒。
            WaitAnim animWait = new WaitAnim(1);
            while (!animWait.update(table, _time))
            {
                yield return null;
            }


            //重新翻回
            if (!startFaceUp)
            {
                cardUI.asAnimator.SetBool("isFaceUp", false);
                AnimAnim animFlipBack = new AnimAnim(cardUI.animator, cardUI._flipToBackAnimName);
                while (!animFlipBack.update(table, _time))
                {
                    yield return null;
                }

            }

            targetTransform = getCardTargetTransfrom(table, record, arg);
            //放回去
            TableAnimation animMoveBack = new CardTransAnim(cardUI, targetTransform, startScale, table.cardShowCurve);
            while (!animMoveBack.update(table, _time))
            {
                yield return null;
            }



            //销毁实例
            if (cardUI)
                UObject.Destroy(cardUI.gameObject);

            if (hand)
            {
                hand.alpha = 1;
            }
            yield break;
        }

        private Transform getCardTargetTransfrom(TableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;

            Transform trans = null;
            var pile = _card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK)
                {
                    trans = (player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck).transform;
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(_card, out var handItem))
                    {
                        trans = handItem.transform;
                    }
                    else
                    {
                        trans = (player == table.player ? table.ui.SelfHandList : table.ui.EnemyHandList).transform;
                    }
                }
                else if (pile.name == PILE.GRAVE)
                {
                    trans = (player == table.player ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck).transform;
                }
            }
            return trans;
        }

        private float getCardStartHeight(TableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;

            float height = 1;
            var pile = _card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK)
                {
                    height = (player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck).getCardHeight();
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(_card, out var handItem))
                    {
                        height = handItem.rectTransform.rect.height * handItem.transform.lossyScale.y;
                    }
                    else
                    {
                        height = (player == table.player ? table.ui.SelfHandList : table.ui.EnemyHandList).rectTransform.rect.height;
                    }
                }
                else if (pile.name == PILE.GRAVE)
                {
                    height = (player == table.player ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck).getCardHeight();
                }
            }
            return height;
        }
    }
}
