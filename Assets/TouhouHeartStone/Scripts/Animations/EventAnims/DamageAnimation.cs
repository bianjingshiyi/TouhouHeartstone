using TouhouHeartstone;
using UI;
using TouhouCardEngine;
using System.Collections;

namespace Game
{
    using Arg = THHCard.DamageEventArg;
    class DamageAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            while (!record.isCompleted)
                yield return null;
            if (record.isCanceled)
                yield break;

            var target = arg.target;
            var damagedValue = arg.damagedValue;

            AnimAnim anim = null;
            if (table.tryGetServant(target, out var servant))
            {
                servant.damageText.text = "-" + damagedValue;
                servant.onDamage.invokeBeforeAnim();
                anim = new AnimAnim(servant.servantAnimator, servant.onDamage.animName);
            }
            else if (table.tryGetMaster(target, out Master master))
            {
                master.DamageText.text = "-" + damagedValue;
                master.onDamage.invokeBeforeAnim();
                anim = new AnimAnim(master.animator, master.onDamage.animName);
            }
            else if (table.tryGetItem(target, out var item))
            {
                item.onLifeDown.invokeBeforeAnim();
                anim = new AnimAnim(item.animator, item.onLifeDown.animName);
            }
            while (anim != null && !anim.update(table, _time))
                yield return null;

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
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
                return true;
            if (nextAnim is TurnEndAnimation)
                return true;
            return false;
        }
    }
}
