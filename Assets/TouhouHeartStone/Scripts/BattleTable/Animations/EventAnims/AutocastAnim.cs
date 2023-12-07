using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = AutocastEventDefine;
    class AutocastAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, EVENTVAR.CARD);
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);

            if (card == null)
                yield break;

            //在对应的卡组上创建卡牌实例

            // 获取卡牌源位置。
            var deck = table.getDeckUI(player.getDeck());
            var targetTransform = deck.transform;
            var startHeight = deck.getCardHeight();

            // 如果该牌在你的手牌里，起始正面朝上。
            var worldPos = targetTransform?.position ?? table.table.CenterDisplayTransform.position;
            var cardPos = table.WorldToTablePosition(targetTransform?.position ?? Vector3.zero);
            TableCard cardUI = table.createInvokeCard(card, cardPos, -1);
            cardUI.setFaceup(false);

            var startScale = Vector3.one * startHeight / cardUI.height;
            cardUI.transform.localScale = startScale;

            //播放位移曲线动画到对应的位置
            Transform targetTrans = table.table.LeftDisplayTransform;
            TableAnimation animMove = new CardTransAnim(cardUI, targetTrans, targetTrans.localScale, table.cardShowCurve);
            while (!animMove.update(table, _deltaTime))
            {
                yield return null;
            }

            //翻转揭示
            AnimAnim anim = null;
            table.setCard(cardUI, card);
            cardUI.animateFaceup(true);
            while (!SimpleAnimHelper.update(table, ref anim, cardUI.onFlipToFace, cardUI.animator, _deltaTime))
            {
                yield return null;
            }

            // 等待1秒。
            WaitAnim animWait = new WaitAnim(1);
            while (!animWait.update(table, _deltaTime))
            {
                yield return null;
            }

            //消失
            AnimAnim disappearAnim = null;
            while (!SimpleAnimHelper.update(table, ref disappearAnim, cardUI.onDisappear, cardUI.animator, _deltaTime))
            {
                yield return null;
            }


            //销毁实例
            if (cardUI)
                cardUI.destroy();

            yield break;
        }
    }
}
