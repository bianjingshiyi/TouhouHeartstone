using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Game.BattleTable
{
    using Arg = RevealEventDefine;
    class RevealAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, EVENTVAR.CARD);
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var shouldAnimate = eventArg.getVar<bool>(Arg.VAR_ANIMATE);

            if (!shouldAnimate || card == null)
            {
                yield break;
            }
            //在对应的卡组上创建卡牌实例

            // 获取卡牌源位置。
            var targetTransform = getCardTargetTransfrom(table, card);
            var startHeight = getCardStartHeight(table, card);

            // 如果该牌在你的手牌里，起始正面朝上。
            bool startFaceUp = player == table.player && card.pile.name == PILE.HAND;
            var worldPos = targetTransform?.position ?? Vector3.zero;
            var cardPos = table.WorldToTablePosition(targetTransform?.position ?? Vector3.zero);
            TableCard cardUI = table.createInvokeCard(card, cardPos, -1);
            cardUI.setFaceup(startFaceUp);


            var startScale = Vector3.one * startHeight / cardUI.height;
            cardUI.transform.localScale = startScale;

            if (table.tryGetHand(card, out var hand))
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
                table.setCard(cardUI, card);
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

            targetTransform = getCardTargetTransfrom(table, card);
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

        public static Transform getCardTargetTransfrom(BattleTableManager table, CardSnapshot card)
        {
            Transform trans = table.table.CenterDisplayTransform;
            var pile = card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK || pile.name == PILE.GRAVE)
                {
                    trans = table.getDeckUI(pile).transform;
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(card, out var handItem))
                    {
                        trans = handItem.transform;
                    }
                    else
                    {
                        trans = table.getHandList(pile).transform;
                    }
                }
            }
            return trans;
        }

        public static float getCardStartHeight(BattleTableManager table, CardSnapshot card)
        {
            float height = 0;
            var pile = card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK || pile.name == PILE.GRAVE)
                {
                    var deck = table.getDeckUI(pile);
                    height = deck.getCardHeight();
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(card, out var handItem))
                    {
                        height = handItem.card.height * handItem.card.transform.lossyScale.z;
                    }
                }
            }
            return height;
        }
    }
}
