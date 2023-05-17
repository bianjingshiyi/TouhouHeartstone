using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UDeck = UI.Deck;
using UObject = UnityEngine.Object;
using Card = UI.Card;
using System.Collections;

namespace Game
{
    using Arg = THHPlayer.RevealDuelEventArg;
    class InvokeEAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
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
            UDeck targetDeck;
            Card cardUISelf = null;
            Card cardUIOppo = null;
            if (youCard != null)
            {
                targetDeck = player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck;
                cardUISelf = table.createInvokeCard(youCard, targetDeck.transform.position, targetDeck.getCardHeight(), false);
            }
            if (oppoCard != null)
            {
                targetDeck = player == table.player ? table.ui.EnemyDeck : table.ui.SelfDeck;
                cardUIOppo = table.createInvokeCard(oppoCard, targetDeck.transform.position, targetDeck.getCardHeight(), false);
            }

            //播放位移曲线动画到对应的位置
            RectTransform targetRect;
            TableAnimation animMoveToSelf = null;
            TableAnimation animMoveToOppo = null;
            if (cardUISelf)
            {
                targetRect = player == table.player ? table.ui.LeftDisplayImage.rectTransform : table.ui.RightDisplayImage.rectTransform;
                animMoveToSelf = new CardTransAnim(cardUISelf, targetRect, Vector3.one * targetRect.rect.height / cardUISelf.rectTransform.rect.height, table.cardShowCurve);
            }
            if (cardUIOppo)
            {
                targetRect = player == table.player ? table.ui.RightDisplayImage.rectTransform : table.ui.LeftDisplayImage.rectTransform;
                animMoveToOppo = new CardTransAnim(cardUIOppo, targetRect, Vector3.one * targetRect.rect.height / cardUIOppo.rectTransform.rect.height, table.cardShowCurve);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _time, animMoveToSelf, animMoveToOppo))
                yield return null;

            //翻转揭示
            AnimAnim animflipFaceSelf = null;
            AnimAnim animflipFaceOppo = null;
            if (cardUISelf && youCard != null)
            {
                animflipFaceSelf = new AnimAnim(cardUISelf.animator, cardUISelf._flipToFaceAnimName);
                table.setCard(cardUISelf, youCard);
            }
            if (cardUIOppo && oppoCard != null)
            {
                animflipFaceOppo = new AnimAnim(cardUIOppo.animator, cardUIOppo._flipToFaceAnimName);
                table.setCard(cardUIOppo, oppoCard);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _time, animflipFaceSelf, animflipFaceOppo))
                yield return null;


            //如果胜利应该有一个胜利动画
            TableAnimation animResult = result ? new AnimAnim(cardUISelf.animator, cardUISelf._notifyAnimName) : (TableAnimation)new WaitAnim(0.5f);
            while (animResult != null && !animResult.update(table, _time))
                yield return null;

            //重新翻回
            AnimAnim animflipBackSelf = null;
            AnimAnim animflipBackOppo = null;
            if (cardUISelf)
            {
                cardUISelf.asAnimator.SetBool("isFaceUp", false);
                animflipBackSelf = new AnimAnim(cardUISelf.animator, cardUISelf._flipToBackAnimName);
            }
            if (cardUIOppo)
            {
                cardUIOppo.asAnimator.SetBool("isFaceUp", false);
                animflipBackOppo = new AnimAnim(cardUIOppo.animator, cardUIOppo._flipToBackAnimName);
            }
            while (AnimHelper.isAnyAnimUpdating(table, _time, animflipBackSelf, animflipBackOppo))
                yield return null;

            //放回牌库
            TableAnimation animMoveBackSelf = null;
            TableAnimation animMoveBackOppo = null;
            if (cardUISelf)
            {
                targetDeck = player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck;
                animMoveBackSelf = new CardTransAnim(cardUISelf, targetDeck.transform, Vector3.one * targetDeck.getCardHeight() / cardUISelf.rectTransform.rect.height, table.cardShowCurve);
            }
            if (cardUIOppo)
            {
                targetDeck = player == table.player ? table.ui.EnemyDeck : table.ui.SelfDeck;
                animMoveBackOppo = new CardTransAnim(cardUIOppo, targetDeck.transform, Vector3.one * targetDeck.getCardHeight() / cardUIOppo.rectTransform.rect.height, table.cardShowCurve);
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
