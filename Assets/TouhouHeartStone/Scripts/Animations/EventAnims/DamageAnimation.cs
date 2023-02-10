using TouhouHeartstone;
using UI;
using System.Collections.Generic;
namespace Game
{
    class DamageAnimation : EventAnimation<THHCard.DamageEventArg>
    {
        List<AnimAnim> _animList = null;
        public override bool update(TableManager table, THHCard.DamageEventArg eventArg, float time)
        {
            if (_animList == null)
            {
                _animList = new List<AnimAnim>();
                var card = eventArg.target;
                if (table.tryGetServant(card, out var servant))
                {
                    servant.damageText.text = "-" + eventArg.damagedValue;
                    servant.onDamage.invokeBeforeAnim();
                    _animList.Add(new AnimAnim(servant.servantAnimator, servant.onDamage.animName));
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.DamageText.text = "-" + eventArg.damagedValue;
                    master.onDamage.invokeBeforeAnim();
                    _animList.Add(new AnimAnim(master.animator, master.onDamage.animName));
                }
                else if (table.tryGetItem(card, out var item))
                {
                    item.onLifeDown.invokeBeforeAnim();
                    _animList.Add(new AnimAnim(item.animator, item.onLifeDown.animName));
                }
            }
            bool isAllAnimDone = true;
            foreach (var anim in _animList)
            {
                if (!anim.update(table, time))
                    isAllAnimDone = false;
            }
            if (!isAllAnimDone)
                return false;

            var target = eventArg.target;
            if (table.tryGetServant(target, out var animServant))
            {
                animServant.onDamage.invokeAfterAnim();
            }
            else if (table.tryGetMaster(target, out Master animMaster))
            {
                animMaster.onDamage.invokeAfterAnim();
            }
            else if (table.tryGetItem(target, out var animItem))
            {
                animItem.onLifeDown.invokeAfterAnim();
            }
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
                return true;
            if (nextAnim is TurnEndAnimation)
                return true;
            //if (nextAnim is DamageAnimation damage && damage.tEventArg.cards.Intersect(tEventArg.cards).Count() > 0)
            //    return true;
            return false;
        }
    }
    public interface IActorAnimation
    {

    }
}
