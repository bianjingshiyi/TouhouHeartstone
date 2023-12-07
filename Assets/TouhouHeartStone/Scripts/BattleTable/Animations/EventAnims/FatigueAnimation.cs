using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = FatigueEventDefine;
    class FatigueAnimation : EventAnimation<Arg>
    {
        AnimAnim _anim;

        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            yield return stopWhenCanceled();

            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var damage = eventArg.getVar<int>(Arg.VAR_DAMAGE);

            var fatigue = table.table.Fatigue;
            fatigue.setDamage(damage, table.lang);
            var deck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
            fatigue.transform.position = deck.HoverTransform.position;
            fatigue.setActive(true);
            fatigue.positionLerp.setStartPositionToCurrent();
            _anim = new AnimAnim(fatigue.animator, fatigue.onFatigue.animName);
            while (!_anim.update(table, _deltaTime))
                yield return null;
            fatigue.setActive(false);
        }
    }
}
