using UnityEngine;
using UI;
using UObject = UnityEngine.Object;
using TouhouHeartstone.Builtin;
using UDeck = UI.Deck;
namespace Game
{
    class InvokeEAnim : EventAnimation<Sanae.RevealDuelArg>
    {
        Card _cardSelf;
        Card _cardOppo;
        TableAnimation _moveToSelf;
        TableAnimation _moveToOppo;
        AnimAnim _flipFaceSelf;
        AnimAnim _flipFaceOppo;
        TableAnimation _resultAnim;
        AnimAnim _flipBackSelf;
        AnimAnim _flipBackOppo;
        TableAnimation _moveBackSelf;
        TableAnimation _moveBackOppo;
        public override bool update(TableManager table, Sanae.RevealDuelArg eventArg, float time)
        {
            if (eventArg.youCard == null && eventArg.oppoCard == null)
                return true;
            //在对应的卡组上创建卡牌实例
            UDeck targetDeck;
            if (_cardSelf == null && eventArg.youCard != null)
            {
                targetDeck = eventArg.player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck;
                _cardSelf = table.createCard(eventArg.youCard, targetDeck.transform.position, targetDeck.getCardHeight(), false);
            }
            if (_cardOppo == null && eventArg.oppoCard != null)
            {
                targetDeck = eventArg.player == table.player ? table.ui.EnemyDeck : table.ui.SelfDeck;
                _cardOppo = table.createCard(eventArg.oppoCard, targetDeck.transform.position, targetDeck.getCardHeight(), false);
            }
            //播放位移曲线动画到对应的位置
            RectTransform targetRect;
            if (_moveToSelf == null && eventArg.youCard != null)
            {
                targetRect = eventArg.player == table.player ? table.ui.LeftDisplayImage.rectTransform : table.ui.RightDisplayImage.rectTransform;
                _moveToSelf = new CardTransAnim(_cardSelf, targetRect, Vector3.one * targetRect.rect.height / _cardSelf.rectTransform.rect.height, table.cardShowCurve);
            }
            if (_moveToOppo == null && eventArg.oppoCard != null)
            {
                targetRect = eventArg.player == table.player ? table.ui.RightDisplayImage.rectTransform : table.ui.LeftDisplayImage.rectTransform;
                _moveToOppo = new CardTransAnim(_cardOppo, targetRect, Vector3.one * targetRect.rect.height / _cardOppo.rectTransform.rect.height, table.cardShowCurve);
            }
            if (AnimHelper.isAnyAnimUpdating(table, time, _moveToSelf, _moveToOppo))
                return false;
            //if ((_moveToSelf == null || !_moveToSelf.update(table, time)) && (_moveToOppo == null || !_moveToOppo.update(table, time)))
            //    return false;
            //bool MoveToDisPlay1 = _moveToSelf != null ? !_moveToSelf.update(table, time) : false;
            //bool MoveToDisPlay2 = _moveToOppo != null ? !_moveToOppo.update(table, time) : false;
            //if (MoveToDisPlay1 || MoveToDisPlay2)
            //    return false;
            //翻转揭示
            if (_flipFaceSelf == null && eventArg.youCard != null)
            {
                _flipFaceSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToFaceAnimName);
                table.setCard(_cardSelf, eventArg.yourCardSnapshot);
            }
            if (_flipFaceOppo == null && eventArg.oppoCard != null)
            {
                _flipFaceOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToFaceAnimName);
                table.setCard(_cardOppo, eventArg.oppoCardSnapshot);
            }
            //bool FlipFace1 = _flipFaceSelf != null ? !_flipFaceSelf.update(table, time) : false;
            //bool FlipFace2 = _flipFaceOppo != null ? !_flipFaceOppo.update(table, time) : false;
            if (AnimHelper.isAnyAnimUpdating(table, time, _flipFaceSelf, _flipFaceOppo))
                return false;
            //if ((_flipFaceSelf == null || !_flipFaceSelf.update(table, time)) && (_flipFaceOppo == null || !_flipFaceOppo.update(table, time)))
            //    return false;

            //如果胜利应该有一个胜利动画
            if (_resultAnim == null)
                _resultAnim = eventArg.result ? new AnimAnim(_cardSelf.animator, _cardSelf._notifyAnimName) : (TableAnimation)new WaitAnim(0.5f);
            if (_resultAnim != null && !_resultAnim.update(table, time))
                return false;
            //重新翻回
            if (_flipBackSelf == null && eventArg.youCard != null)
            {
                _cardSelf.asAnimator.SetBool("isFaceUp", false);
                _flipBackSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToBackAnimName);
            }
            if (_flipBackOppo == null && eventArg.oppoCard != null)
            {
                _cardOppo.asAnimator.SetBool("isFaceUp", false);
                _flipBackOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
            }
            if (AnimHelper.isAnyAnimUpdating(table, time, _flipBackSelf, _flipBackOppo))
                return false;
            //bool FlipBack1 = _flipBackSelf != null ? !_flipBackSelf.update(table, time) : false;
            //bool Flipback2 = _flipBackOppo != null ? !_flipBackOppo.update(table, time) : false;
            //if (FlipBack1 || Flipback2)
            //{
            //    return false;
            //}

            //放回牌库
            if (_moveBackSelf == null && eventArg.youCard != null)
            {
                targetDeck = eventArg.player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck;
                _moveBackSelf = new CardTransAnim(_cardSelf, targetDeck.transform, Vector3.one * targetDeck.getCardHeight() / _cardSelf.rectTransform.rect.height, table.cardShowCurve);
            }
            if (_moveBackOppo == null && eventArg.oppoCard != null)
            {
                targetDeck = eventArg.player == table.player ? table.ui.EnemyDeck : table.ui.SelfDeck;
                _moveBackOppo = new CardTransAnim(_cardOppo, targetDeck.transform, Vector3.one * targetDeck.getCardHeight() / _cardOppo.rectTransform.rect.height, table.cardShowCurve);
            }
            if (AnimHelper.isAnyAnimUpdating(table, time, _moveBackSelf, _moveBackOppo))
                return false;
            //bool MoveBack1 = _moveBackSelf != null ? !_moveBackSelf.update(table, time) : false;
            //bool MoveBack2 = _moveBackOppo != null ? !_moveBackOppo.update(table, time) : false;
            //if (MoveBack2 || MoveBack1)
            //{
            //    return false;
            //}
            //销毁实例
            if (_cardSelf != null)
                UObject.Destroy(_cardSelf.gameObject);
            if (_cardOppo != null)
                UObject.Destroy(_cardOppo.gameObject);
            return true;
        }
    }
}
