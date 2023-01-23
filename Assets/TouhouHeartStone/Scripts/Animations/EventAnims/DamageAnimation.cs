using TouhouHeartstone;
using BJSYGameCore;
using System.Linq;
using UI;
using System.Collections.Generic;
using Card = TouhouCardEngine.Card;
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
                foreach (var card in eventArg.infoDic.Keys)
                {
                    if (!eventArg.infoDic.TryGetValue(card, out var info))
                    {
                        continue;
                    }
                    if (table.tryGetServant(card, out var servant))
                    {
                        servant.damageText.text = "-" + info.damagedValue;
                        servant.lifePropNumber.text.text = info.currentLife.ToString();
                        servant.onDamage.invokeBeforeAnim();
                        _animList.Add(new AnimAnim(servant.servantAnimator, servant.onDamage.animName));
                    }
                    else if (table.tryGetMaster(card, out Master master))
                    {
                        master.DamageText.text = "-" + info.damagedValue;
                        master.LifePropNumber.text.text = info.currentLife.ToString();
                        master.onDamage.invokeBeforeAnim();
                        _animList.Add(new AnimAnim(master.animator, master.onDamage.animName));
                    }
                    else if (table.tryGetItem(card, out var item))
                    {
                        item.DurabilityPropNumber.text.text = info.currentLife.ToString();
                        item.onLifeDown.invokeBeforeAnim();
                        _animList.Add(new AnimAnim(item.animator, item.onLifeDown.animName));
                    }
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
            foreach (var card in eventArg.infoDic.Keys)
            {
                if (table.tryGetServant(card, out var servant))
                {
                    servant.onDamage.invokeAfterAnim();
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.onDamage.invokeAfterAnim();
                }
                else if (table.tryGetItem(card, out var item))
                {
                    item.onLifeDown.invokeAfterAnim();
                }
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
