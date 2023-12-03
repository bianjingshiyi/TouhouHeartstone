using System.Collections;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = THHCard.MissionCompleteEventArg;
    public class MissionTriggerAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(tEventArg, EVENTVAR.CARD);
            if (table.tryGetSecret(card, out var secret))
            {
                secret.dummyTarget.position = table.table.CenterDisplayTransform.position;

                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, secret.onMissionCompleted, secret.animator, _deltaTime))
                {
                    table.setCard(secret.card, card, true);
                    yield return null;
                }
            }
        }
    }
}