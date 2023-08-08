using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = THHCard.DamageEventArg;
    class DamageAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();

            var target = arg.target;
            var damagedValue = arg.damagedValue;

            AnimAnim anim = null;
            if (table.tryGetServant(target, out var servant))
            {
                servant.showDamageText(damagedValue);
                anim = new AnimAnim(servant.animator, servant.onDamage.animName);
            }
            else if (table.tryGetMaster(target, out Master master))
            {
                master.showDamageText(damagedValue);
                anim = new AnimAnim(master.animator, master.onDamage.animName);
            }
            else if (table.tryGetItem(target, out var item))
            {
                item.onLifeDown.invokeBeforeAnim();
                anim = new AnimAnim(item.animator, item.onLifeDown.animName);
            }
            while (anim != null && !anim.update(table, _deltaTime))
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
