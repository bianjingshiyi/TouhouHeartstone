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
        public override bool update(TableManager table, Sanae.RevealDuelArg eventArg)
        {
            //在对应的卡组上创建卡牌实例
            _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
            table.setCard(_cardSelf, eventArg.youCard, true);
            _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
            table.setCard(_cardOppo, eventArg.oppoCard, true);
            //播放位移曲线动画到对应的位置
            if (_moveToSelf == null)
                _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            if (_moveToOppo == null)
                _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            if (!_moveToSelf.update(table) || !_moveToOppo.update(table))
                return false;
            //翻转揭示
            if (_flipFaceSelf == null)
                _flipFaceSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToFaceAnimName);
            if (_flipFaceOppo == null)
                _flipFaceOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
            if (!_flipFaceSelf.update(table) || !_flipFaceOppo.update(table))
                return false;
            //如果胜利应该有一个胜利动画
            if (eventArg.result == true)
                _winAnim = new AnimAnim(_cardSelf.animator, _cardSelf._notifyAnimName);
            if (_winAnim != null && !_winAnim.update(table))
                return false;
            //重新翻回
            if (_flipBackSelf == null)
                _flipBackSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToBackAnimName);
            if (_flipBackOppo == null)
                _flipBackOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
            if (!_flipBackSelf.update(table) || !_flipBackOppo.update(table))
                return false;
            //放回牌库
            if (_moveBackSelf == null)
                _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            if (_moveBackOppo == null)
                _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            if (!_moveBackSelf.update(table) || !_moveBackOppo.update(table))
                return false;
            //销毁实例
            UObject.Destroy(_cardSelf);
            UObject.Destroy(_cardOppo);
            return true;
        }
    }
}
