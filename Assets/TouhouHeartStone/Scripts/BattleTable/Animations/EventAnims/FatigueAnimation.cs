using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.FatigueEventArg;
    class FatigueAnimation : EventAnimation<Arg>
    {
        AnimAnim _anim;

        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return stopWhenCanceled();

            var player = arg.player;
            var damage = arg.damage;

            var fatigue = table.table.Fatigue;
            fatigue.setDamage(damage, table.lang);
            var deck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
            fatigue.transform.position = deck.HoverTransform.position;
            fatigue.setActive(true);
            fatigue.positionLerp.setStart();
            _anim = new AnimAnim(fatigue.animator, fatigue.onFatigue.animName);
            while (!_anim.update(table, _deltaTime))
                yield return null;
            fatigue.setActive(false);
        }
    }
}
