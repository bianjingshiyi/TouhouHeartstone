using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = THHCard.HealEventArg;
    class HealAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            if (record == null || record.isCanceled)
                yield break;
            var card = arg.card;
            var source = arg.source;
            var value = arg.value;
            var healedValue = arg.healedValue;


            var animList = new List<AnimAnim>();
            if (table.tryGetServant(card, out Servant servant))
            {
                servant.showHealText(healedValue);
                animList.Add(new AnimAnim(servant.animator, servant.onHeal.animName));
            }
            else if (table.tryGetMaster(card, out Master master))
            {
                servant.showHealText(healedValue);
                animList.Add(new AnimAnim(master.animator, master.onHeal.animName));
            }
            while (true)
            {
                bool isAllAnimDone = true;
                foreach (var anim in animList)
                {
                    if (!anim.update(table, _time))
                        isAllAnimDone = false;
                }
                if (isAllAnimDone)
                    break;
                yield return null;
            }
            if (table.tryGetServant(card, out var servant2))
            {
                servant2.onHeal.invokeAfterAnim();
            }
            else if (table.tryGetMaster(card, out Master master))
            {
                master.onHeal.invokeAfterAnim();
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
