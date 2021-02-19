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
            if (_cardSelf == null && eventArg.youCard != null)
            {
                if (eventArg.player.name == "自己")
                    _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
                else
                    _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
                _cardSelf.asAnimator.SetBool("isFaceUp", false);
            }
            if(_cardSelf != null)
                table.setCard(_cardSelf, eventArg.youCard, _cardSelf.asAnimator.GetBool("isFaceUp"));
            if (_cardOppo == null && eventArg.oppoCard != null)
            {
                if (eventArg.player.name == "自己")
                    _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
                else
                    _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
                _cardOppo.asAnimator.SetBool("isFaceUp", false);
            }
            if (_cardOppo != null)
                table.setCard(_cardOppo, eventArg.oppoCard, _cardOppo.asAnimator.GetBool("isFaceUp"));

            //播放位移曲线动画到对应的位置
            if (_moveToSelf == null && eventArg.youCard != null)
            {
                if (eventArg.player.name == "自己")
                    _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
                else
                    _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            }
            if (_moveToOppo == null && eventArg.oppoCard != null)
            {
                if(eventArg.player.name == "自己")
                    _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
                else
                    _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            }
            bool MoveToDisPlay1 = _moveToSelf!=null?!_moveToSelf.update(table, time):false;
            bool MoveToDisPlay2 = _moveToOppo!=null?!_moveToOppo.update(table, time):false;
            if (MoveToDisPlay1 || MoveToDisPlay2)
            {
                return false;
            }
            //翻转揭示
            if (_flipFaceSelf == null && eventArg.youCard != null)
            {
                _flipFaceSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToFaceAnimName);
                _cardSelf.asAnimator.SetBool("isFaceUp", true);
                table.setCard(_cardSelf, eventArg.youCard, _cardSelf.asAnimator.GetBool("isFaceUp"));
            }
            if (_flipFaceOppo == null && eventArg.oppoCard != null)
            {
                _flipFaceOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
                _cardOppo.asAnimator.SetBool("isFaceUp", true);
                table.setCard(_cardOppo, eventArg.oppoCard, _cardOppo.asAnimator.GetBool("isFaceUp"));
            }
            bool FlipFace1 = _flipFaceSelf!=null?!_flipFaceSelf.update(table, time):false;
            bool FlipFace2 = _flipFaceOppo!=null?!_flipFaceOppo.update(table, time):false;
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
            bool FlipBack1 = _flipBackSelf!=null ? !_flipBackSelf.update(table, time):false;
            bool Flipback2 = _flipBackOppo!=null ? !_flipBackOppo.update(table, time):false;
            if (FlipBack1||Flipback2)
            {
                return false;
            }

            //放回牌库
            if (_moveBackSelf == null && eventArg.youCard != null)
            {
                if(eventArg.player.name == "自己")
                    _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.SelfDeck.transform, table.cardShowCurve);
                else
                    _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.EnemyDeck.transform, table.cardShowCurve);
            }
            if (_moveBackOppo == null && eventArg.oppoCard != null)
            {
                if(eventArg.player.name == "自己")
                    _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.EnemyDeck.transform, table.cardShowCurve);
                else
                    _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.SelfDeck.transform, table.cardShowCurve);
            }
            bool MoveBack1 = _moveBackSelf != null ? !_moveBackSelf.update(table, time) : false;
            bool MoveBack2 = _moveBackOppo != null ? !_moveBackOppo.update(table, time) : false;
            if (MoveBack2||MoveBack1)
            {
                return false;
            }
            //销毁实例
            if(_cardSelf!=null)
            UObject.Destroy(_cardSelf.gameObject);
            if(_cardOppo!=null)
            UObject.Destroy(_cardOppo.gameObject);
            return true;
        }
    }
}
