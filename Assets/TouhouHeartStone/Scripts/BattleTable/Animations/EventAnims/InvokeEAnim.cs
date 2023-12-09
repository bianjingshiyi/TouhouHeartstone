using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Game.BattleTable
{
    using Arg = THHPlayer.RevealDuelEventArg;
    class InvokeEAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var youCard = table.getEventCardSnapshot(tEventArg, Arg.VAR_SELF_CARD);
            var oppoCard = table.getEventCardSnapshot(tEventArg, Arg.VAR_OPPO_CARD);
            var player = tEventArg.getVar<Player>(EVENTVAR.PLAYER);
            var result = tEventArg.getVar<bool>(Arg.VAR_RESULT);

            if (youCard == null && oppoCard == null)
                yield break;
            //在对应的卡组上创建卡牌实例
            Deck targetDeck;
            TableCard cardUISelf = null;
            TableCard cardUIOppo = null;
            if (youCard != null)
            {
                targetDeck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
                var deckPos = table.WorldToTablePosition(targetDeck.HoverTransform.position);
                var lossyScale = table.WorldToTableScale(targetDeck.transform.lossyScale);
                cardUISelf = table.createInvokeCard(youCard, deckPos, targetDeck.getCardHeight() * lossyScale.z);
                cardUISelf.setFaceup(false);
            }
            if (oppoCard != null)
            {
                targetDeck = player == table.player ? table.table.EnemyDeck : table.table.SelfDeck;
                var deckPos = table.WorldToTablePosition(targetDeck.HoverTransform.position);
                var lossyScale = table.WorldToTableScale(targetDeck.transform.lossyScale);
                cardUIOppo = table.createInvokeCard(oppoCard, deckPos, targetDeck.getCardHeight() * lossyScale.z);
                cardUIOppo.setFaceup(false);
            }

            //播放位移曲线动画到对应的位置
            Transform targetTrans;
            TableAnimation animMoveToSelf = null;
            TableAnimation animMoveToOppo = null;
            if (cardUISelf)
            {
                targetTrans = player == table.player ? table.table.LeftDisplayTransform : table.table.RightDisplayTransform;
                animMoveToSelf = new CardTransAnim(cardUISelf, targetTrans, targetTrans.localScale, table.cardShowCurve);
            }
            if (cardUIOppo)
            {
                targetTrans = player == table.player ? table.table.RightDisplayTransform : table.table.LeftDisplayTransform;
                animMoveToOppo = new CardTransAnim(cardUIOppo, targetTrans, targetTrans.localScale, table.cardShowCurve);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _deltaTime, animMoveToSelf, animMoveToOppo))
                yield return null;

            //翻转揭示
            AnimAnim animflipFaceSelf = null;
            AnimAnim animflipFaceOppo = null;
            if (cardUISelf && youCard != null)
            {
                animflipFaceSelf = new AnimAnim(cardUISelf.animator, cardUISelf.onFlipToFace.animName);
                table.setCard(cardUISelf, youCard);
                cardUISelf.animateFaceup(true);
            }
            if (cardUIOppo && oppoCard != null)
            {
                animflipFaceOppo = new AnimAnim(cardUIOppo.animator, cardUIOppo.onFlipToFace.animName);
                table.setCard(cardUIOppo, oppoCard);
                cardUIOppo.animateFaceup(true);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _deltaTime, animflipFaceSelf, animflipFaceOppo))
                yield return null;


            //如果胜利应该有一个胜利动画
            TableAnimation animResult = result ? new AnimAnim(cardUISelf.animator, cardUISelf.onNotify.animName) : (TableAnimation)new WaitAnim(0.5f);
            while (animResult != null && !animResult.update(table, _deltaTime))
                yield return null;

            //重新翻回
            AnimAnim animflipBackSelf = null;
            AnimAnim animflipBackOppo = null;
            if (cardUISelf)
            {
                cardUISelf.animateFaceup(false);
                animflipBackSelf = new AnimAnim(cardUISelf.animator, cardUISelf.onFlipToBack.animName);
            }
            if (cardUIOppo)
            {
                cardUIOppo.animateFaceup(false);
                animflipBackOppo = new AnimAnim(cardUIOppo.animator, cardUIOppo.onFlipToBack.animName);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _deltaTime, animflipBackSelf, animflipBackOppo))
                yield return null;

            //放回牌库
            TableAnimation animMoveBackSelf = null;
            TableAnimation animMoveBackOppo = null;
            if (cardUISelf)
            {
                targetDeck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
                animMoveBackSelf = new CardTransAnim(cardUISelf, targetDeck.HoverTransform, Vector3.one * targetDeck.getCardHeight() / cardUISelf.height, table.cardShowCurve);
            }
            if (cardUIOppo)
            {
                targetDeck = player == table.player ? table.table.EnemyDeck : table.table.SelfDeck;
                animMoveBackOppo = new CardTransAnim(cardUIOppo, targetDeck.HoverTransform, Vector3.one * targetDeck.getCardHeight() / cardUIOppo.height, table.cardShowCurve);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _deltaTime, animMoveBackSelf, animMoveBackOppo))
                yield return null;
            //销毁实例
            if (cardUISelf)
                UObject.Destroy(cardUISelf.gameObject);
            if (cardUIOppo)
                UObject.Destroy(cardUIOppo.gameObject);
        }
    }
}
