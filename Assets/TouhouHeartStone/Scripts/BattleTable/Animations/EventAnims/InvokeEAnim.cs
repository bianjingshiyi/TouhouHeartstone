using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UObject = UnityEngine.Object;
using System.Collections;

namespace Game.BattleTable
{
    using Arg = THHPlayer.RevealDuelEventArg;
    class InvokeEAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            if (record == null || record.isCanceled)
                yield break;
            var player = arg.player;
            var youCard = table.getRecordCardSnapshot(record, Arg.VAR_SELF_CARD);
            var oppoCard = table.getRecordCardSnapshot(record, Arg.VAR_OPPO_CARD);
            var result = arg.result;

            if (youCard == null && oppoCard == null)
                yield break;
            //在对应的卡组上创建卡牌实例
            Deck targetDeck;
            TableCard cardUISelf = null;
            TableCard cardUIOppo = null;
            if (youCard != null)
            {
                targetDeck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
                cardUISelf = table.createInvokeCard(youCard, targetDeck.transform.position, targetDeck.getCardHeight(), false);
            }
            if (oppoCard != null)
            {
                targetDeck = player == table.player ? table.table.EnemyDeck : table.table.SelfDeck;
                cardUIOppo = table.createInvokeCard(oppoCard, targetDeck.transform.position, targetDeck.getCardHeight(), false);
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
            while (AnimHelper.isAnyAnimUpdating(table, _time, animMoveToSelf, animMoveToOppo))
                yield return null;

            //翻转揭示
            AnimAnim animflipFaceSelf = null;
            AnimAnim animflipFaceOppo = null;
            if (cardUISelf && youCard != null)
            {
                animflipFaceSelf = new AnimAnim(cardUISelf.animator, cardUISelf.flipToFaceAnimName);
                table.setCard(cardUISelf, youCard);
            }
            if (cardUIOppo && oppoCard != null)
            {
                animflipFaceOppo = new AnimAnim(cardUIOppo.animator, cardUIOppo.flipToFaceAnimName);
                table.setCard(cardUIOppo, oppoCard);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _time, animflipFaceSelf, animflipFaceOppo))
                yield return null;


            //如果胜利应该有一个胜利动画
            TableAnimation animResult = result ? new AnimAnim(cardUISelf.animator, cardUISelf.notifyAnimName) : (TableAnimation)new WaitAnim(0.5f);
            while (animResult != null && !animResult.update(table, _time))
                yield return null;

            //重新翻回
            AnimAnim animflipBackSelf = null;
            AnimAnim animflipBackOppo = null;
            if (cardUISelf)
            {
                cardUISelf.setFaceup(false);
                animflipBackSelf = new AnimAnim(cardUISelf.animator, cardUISelf.flipToBackAnimName);
            }
            if (cardUIOppo)
            {
                cardUIOppo.setFaceup(false);
                animflipBackOppo = new AnimAnim(cardUIOppo.animator, cardUIOppo.flipToBackAnimName);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _time, animflipBackSelf, animflipBackOppo))
                yield return null;

            //放回牌库
            TableAnimation animMoveBackSelf = null;
            TableAnimation animMoveBackOppo = null;
            if (cardUISelf)
            {
                targetDeck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
                animMoveBackSelf = new CardTransAnim(cardUISelf, targetDeck.transform, Vector3.one * targetDeck.getCardHeight() / cardUISelf.height, table.cardShowCurve);
            }
            if (cardUIOppo)
            {
                targetDeck = player == table.player ? table.table.EnemyDeck : table.table.SelfDeck;
                animMoveBackOppo = new CardTransAnim(cardUIOppo, targetDeck.transform, Vector3.one * targetDeck.getCardHeight() / cardUIOppo.height, table.cardShowCurve);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _time, animMoveBackSelf, animMoveBackOppo))
                yield return null;
            //销毁实例
            if (cardUISelf)
                UObject.Destroy(cardUISelf.gameObject);
            if (cardUIOppo)
                UObject.Destroy(cardUIOppo.gameObject);
        }
    }
}
