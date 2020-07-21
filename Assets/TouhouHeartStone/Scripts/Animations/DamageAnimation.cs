using TouhouHeartstone;
using BJSYGameCore;
using System.Linq;
using UI;
using System.Collections.Generic;
namespace Game
{
    class DamageAnimation : EventAnimation<THHCard.DamageEventArg>
    {
        List<AnimAnim> _animList = null;
        public override bool update(TableManager table, THHCard.DamageEventArg eventArg)
        {
            if (_animList == null)
            {
                _animList = new List<AnimAnim>();
                foreach (var card in eventArg.infoDic.Keys)
                {
                    if (table.tryGetServant(card, out var servant))
                    {
                        servant.DamageText.text = "-" + eventArg.value;
                        servant.HpTextPropNumber.asText.text = eventArg.infoDic[card].currentLife.ToString();
                        servant.onDamage.beforeAnim.Invoke();
                        _animList.Add(new AnimAnim(servant.animator, servant.onDamage.animName));
                    }
                    else if (table.tryGetMaster(card, out Master master))
                    {
                        master.DamageText.text = "-" + eventArg.value;
                        master.LifePropNumber.asText.text = eventArg.infoDic[card].currentLife.ToString();
                        master.onDamage.beforeAnim.Invoke();
                        _animList.Add(new AnimAnim(master.animator, master.onDamage.animName));
                    }
                    else if (table.tryGetItem(card, out var item))
                    {
                        item.DurabilityPropNumber.asText.text = eventArg.infoDic[card].currentLife.ToString();
                        item.onLifeDown.beforeAnim.Invoke();
                        _animList.Add(new AnimAnim(item.animator, item.onLifeDown.animName));
                    }
                }
            }
            bool isAllAnimDone = true;
            foreach (var anim in _animList)
            {
                if (!anim.update(table))
                    isAllAnimDone = false;
            }
            if (!isAllAnimDone)
                return false;
            foreach (var card in eventArg.infoDic.Keys)
            {
                if (table.tryGetServant(card, out var servant))
                {
                    servant.onDamage.afterAnim.Invoke();
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.onDamage.afterAnim.Invoke();
                }
                else if (table.tryGetItem(card, out var item))
                {
                    item.onLifeDown.afterAnim.Invoke();
                }
            }
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
                return true;
            if (nextAnim is DamageAnimation damage && damage.tEventArg.cards.Intersect(tEventArg.cards).Count() > 0)
                return true;
            return false;
        }
    }
    public interface IActorAnimation
    {

    }
}
