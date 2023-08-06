using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = THHCard.MissionCompleteEventArg;
    public class MissionTriggerAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);
            if (table.tryGetSecret(card, out var secret))
            {
                secret.dummyTarget.position = table.table.CenterDisplayTransform.position;

                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, secret.onTrigger, secret.animator, _deltaTime))
                {
                    table.setCard(secret.card, card, true);
                    yield return null;
                }
            }
        }
    }
}