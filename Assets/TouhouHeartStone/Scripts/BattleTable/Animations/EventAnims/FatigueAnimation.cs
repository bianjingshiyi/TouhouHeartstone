using TouhouHeartstone;
using UI;
using TouhouCardEngine;
using System.Collections;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.FatigueEventArg;
    class FatigueAnimation : EventAnimation<Arg>
    {
        AnimAnim _anim;

        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            if (record.isCanceled)
                yield break;

            var player = arg.player;
            var damage = arg.damage;

            var fatigue = table.table.Fatigue;
            fatigue.setDamage(damage, table.lang);
            var deck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
            fatigue.transform.position = deck.transform.position;
            fatigue.setActive(true);
            _anim = new AnimAnim(fatigue.animator, deck.onFatigue.animName);
            while (!_anim.update(table, _time))
                yield return null;
            fatigue.setActive(false);
        }
    }
}
