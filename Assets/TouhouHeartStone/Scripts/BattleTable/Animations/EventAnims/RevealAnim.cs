using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.RevealEventArg;
    class RevealAnim : EventAnimation<Arg>
    {
        private CardSnapshot _card;
        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
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
            var worldPos = targetTransform?.position ?? Vector3.zero;
            var cardPos = table.WorldToTablePosition(targetTransform?.position ?? Vector3.zero);
            TableCard cardUI = table.createInvokeCard(_card, cardPos, -1, startFaceUp);


            var startScale = Vector3.one * startHeight / cardUI.height;
            cardUI.transform.localScale = startScale;

            if (table.tryGetHand(_card, out var hand))
            {
                hand.card.setVisible(false);
            }

            //播放位移曲线动画到对应的位置
            Transform targetTrans = table.table.LeftDisplayTransform;
            TableAnimation animMove = new CardTransAnim(cardUI, targetTrans, targetTrans.localScale, table.cardShowCurve);
            while (!animMove.update(table, _deltaTime))
            {
                yield return null;
            }


            //翻转揭示
            if (!startFaceUp)
            {
                AnimAnim anim = null;
                table.setCard(cardUI, _card);
                cardUI.animateFaceup(true);
                while (!SimpleAnimHelper.update(table, ref anim, cardUI.onFlipToFace, cardUI.animator, _deltaTime))
                {
                    yield return null;
                }
            }

            // 等待1秒。
            WaitAnim animWait = new WaitAnim(1);
            while (!animWait.update(table, _deltaTime))
            {
                yield return null;
            }


            //重新翻回
            if (!startFaceUp)
            {
                AnimAnim anim = null;
                cardUI.animateFaceup(false);
                while (!SimpleAnimHelper.update(table, ref anim, cardUI.onFlipToBack, cardUI.animator, _deltaTime))
                {
                    yield return null;
                }

            }

            targetTransform = getCardTargetTransfrom(table, record, arg);
            //放回去
            TableAnimation animMoveBack = new CardTransAnim(cardUI, targetTransform, startScale, table.cardShowCurve);
            while (!animMoveBack.update(table, _deltaTime))
            {
                yield return null;
            }



            //销毁实例
            if (cardUI)
                UObject.Destroy(cardUI.gameObject);

            if (hand)
            {
                hand.card.setVisible(true);
            }
            yield break;
        }

        private Transform getCardTargetTransfrom(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;

            Transform trans = null;
            var pile = _card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK)
                {
                    trans = (player == table.player ? table.table.SelfDeck : table.table.EnemyDeck).HoverTransform;
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(_card, out var handItem))
                    {
                        trans = handItem.transform;
                    }
                    else
                    {
                        trans = (player == table.player ? table.table.SelfHandList : table.table.EnemyHandList).transform;
                    }
                }
                else if (pile.name == PILE.GRAVE)
                {
                    trans = (player == table.player ? table.table.SelfGrave : table.table.EnemyGrave).HoverTransform;
                }
            }
            return trans;
        }

        private float getCardStartHeight(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;

            float height = table.TableToWorldDistance(table.cardHeight);
            var pile = _card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK)
                {
                    var deck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
                    height = deck.getCardHeight();
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(_card, out var handItem))
                    {
                        height = handItem.card.height * handItem.card.transform.lossyScale.z;
                    }
                }
                else if (pile.name == PILE.GRAVE)
                {
                    var grave = player == table.player ? table.table.SelfGrave : table.table.EnemyGrave;
                    height = grave.getCardHeight();
                }
            }
            return height;
        }
    }
}
