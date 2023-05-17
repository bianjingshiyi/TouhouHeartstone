using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    using Arg = THHCard.MissionCompleteEventArg;
    public class MissionTriggerAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);
            if (table.tryGetSecret(card, out var secret))
            {
                secret.getChild("DummyTarget").GetComponent<RectTransform>().position = table.ui.CenterDisplayImage.rectTransform.position;

                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, secret.onTrigger, secret.animator, _time))
                {
                    table.setCard(secret.Card, card, true);
                    yield return null;
                }
            }
        }
    }
}