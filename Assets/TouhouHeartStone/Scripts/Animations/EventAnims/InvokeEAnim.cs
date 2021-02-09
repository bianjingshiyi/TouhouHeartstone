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
                if(eventArg.player.name=="自己")
                    _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
                else
                    _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
                _cardSelf.asAnimator.SetBool("isFaceUp", false);
            }
            table.setCard(_cardSelf, eventArg.youCard, _cardSelf.asAnimator.GetBool("isFaceUp"));
            if (_cardOppo == null)
            {
                if (eventArg.player.name == "自己")
                    _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
                else
                    _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
                _cardOppo.asAnimator.SetBool("isFaceUp", false);
            }
            table.setCard(_cardOppo, eventArg.oppoCard, _cardOppo.asAnimator.GetBool("isFaceUp"));

            //播放位移曲线动画到对应的位置
            if (_moveToSelf == null)
            {
                if (eventArg.player.name == "自己")
                    _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
                else
                    _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            }
            if (_moveToOppo == null)
            {
                if(eventArg.player.name == "自己")
                    _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
                else
                    _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            }
            bool MoveToDisPlay1 = !_moveToSelf.update(table, time);
            bool MoveToDisPlay2 = !_moveToOppo.update(table, time);
            if (MoveToDisPlay1 || MoveToDisPlay2)
            {
                return false;
            }
            //翻转揭示
            if (_flipFaceSelf == null)
            {
                _flipFaceSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToFaceAnimName);
                _cardSelf.asAnimator.SetBool("isFaceUp", true);
                table.setCard(_cardOppo, eventArg.oppoCard, _cardOppo.asAnimator.GetBool("isFaceUp"));
            }
            if (_flipFaceOppo == null)
            {
                _flipFaceOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
                _cardOppo.asAnimator.SetBool("isFaceUp", true);
                table.setCard(_cardOppo, eventArg.oppoCard, _cardOppo.asAnimator.GetBool("isFaceUp"));
            }
            bool FlipFace1 = !_flipFaceSelf.update(table, time);
            bool FlipFace2 = !_flipFaceOppo.update(table, time);
            if (FlipFace1||FlipFace2)
            {
                return false;
            }


            //如果胜利应该有一个胜利动画
            if (eventArg.result == true && _winAnim == null)
                _winAnim = new AnimAnim(_cardSelf.animator, _cardSelf._notifyAnimName);
            if (eventArg.result == false && _winAnim == null) 
                _winAnim = new AnimAnim(_cardOppo.animator, _cardOppo._notifyAnimName);
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
            bool FlipBack1 = !_flipBackSelf.update(table, time);
            bool Flipback2 = !_flipBackOppo.update(table, time);
            if (FlipBack1||Flipback2)
            {
                return false;
            }

            //放回牌库
            if (_moveBackSelf == null)
            {
                if(eventArg.player.name == "自己")
                    _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.SelfDeck.transform, table.cardShowCurve);
                else
                    _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.EnemyDeck.transform, table.cardShowCurve);
            }
            if (_moveBackOppo == null)
            {
                if(eventArg.player.name == "自己")
                    _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.EnemyDeck.transform, table.cardShowCurve);
                else
                    _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.SelfDeck.transform, table.cardShowCurve);
            }
            bool MoveBack1 = !_moveBackSelf.update(table, time);
            bool MoveBack2 = !_moveBackOppo.update(table, time);
            if (MoveBack2||MoveBack1)
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
