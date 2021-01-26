using UnityEngine;
using UI;
using UObject = UnityEngine.Object;
using TouhouHeartstone.Builtin;

namespace Game
{
    class InvokeEAnim : EventAnimation<Sanae.RevealDuelArg>
    {
        Card _cardSelf;
        Card _cardOppo;
        CardMoveAnim _moveToSelf;
        CardMoveAnim _moveToOppo;
        AnimAnim _flipFaceSelf;
        AnimAnim _flipFaceOppo;
        AnimAnim _winAnim;
        AnimAnim _flipBackSelf;
        AnimAnim _flipBackOppo;
        CardMoveAnim _moveBackSelf;
        CardMoveAnim _moveBackOppo;
        public override bool update(TableManager table, Sanae.RevealDuelArg eventArg, float time)
        {
            //在对应的卡组上创建卡牌实例
            if (_cardSelf == null)
            {
                _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
                _cardSelf.asAnimator.SetBool("isFaceUp", false);
            }
            table.setCard(_cardSelf, eventArg.youCard, _cardSelf.asAnimator.GetBool("isFaceUp"));
            if (_cardOppo == null)
            {
                _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
                _cardOppo.asAnimator.SetBool("isFaceUp", false);
            }
            table.setCard(_cardOppo, eventArg.oppoCard, _cardOppo.asAnimator.GetBool("isFaceUp"));
            //播放位移曲线动画到对应的位置
            if (_moveToSelf == null)
                _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            if (_moveToOppo == null)
                _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            if (!_moveToSelf.update(table, time) || !_moveToOppo.update(table, time))
            {
                return false;
            }
            //翻转揭示
            if (_flipFaceSelf == null)
            {
                _flipFaceSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToFaceAnimName);
                _cardSelf.asAnimator.SetBool("isFaceUp", true);
            }
            if (_flipFaceOppo == null)
            {
                _flipFaceOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
                _cardOppo.asAnimator.SetBool("isFaceUp", true);
            }
            if (!_flipFaceSelf.update(table, time) || !_flipFaceOppo.update(table, time))
            {
                return false;
            }


            //如果胜利应该有一个胜利动画
            if (eventArg.result == true && _winAnim == null)
                _winAnim = new AnimAnim(_cardSelf.animator, _cardSelf._notifyAnimName);
            if (_winAnim != null && !_winAnim.update(table, time))
            {
                return false;
            }
            //重新翻回
            if (_flipBackSelf == null)
            {
                _cardSelf.asAnimator.SetBool("isFaceUp", false);
                _flipBackSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToBackAnimName);
            }
            if (_flipBackOppo == null)
            {
                _cardOppo.asAnimator.SetBool("isFaceUp", false);
                _flipBackOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
            }

            if (!_flipBackSelf.update(table, time) || !_flipBackOppo.update(table, time))
            {
                return false;
            }

            //放回牌库
            if (_moveBackSelf == null)
                _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            if (_moveBackOppo == null)
                _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            if (!_moveBackSelf.update(table, time) || !_moveBackOppo.update(table, time))
            {
                return false;
            }
            //销毁实例
            UObject.Destroy(_cardSelf.gameObject);
            UObject.Destroy(_cardOppo.gameObject);
            return true;
        }
    }
}
