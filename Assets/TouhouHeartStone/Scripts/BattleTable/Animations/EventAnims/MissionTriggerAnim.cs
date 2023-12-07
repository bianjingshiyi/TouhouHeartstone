using System.Collections;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = MissionCompleteEventDefine;
    public class MissionTriggerAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, EVENTVAR.CARD);
            if (table.tryGetSecret(card, out var secret))
            {
                secret.dummyTarget.position = table.table.CenterDisplayTransform.position;

                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, secret.onMissionCompleted, secret.animator, _deltaTime))
                {
                    table.setCard(secret.card, card);
                    secret.card.setFaceup(true);
                    yield return null;
                }
            }
        }
    }
}