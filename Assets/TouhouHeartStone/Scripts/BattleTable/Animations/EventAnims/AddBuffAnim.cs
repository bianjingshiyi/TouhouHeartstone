using System.Collections;
using TouhouCardEngine;

namespace Game.BattleTable
{
    using Arg = Card.AddBuffEventArg;
    class AddBuffAnim : EventAnimation<Arg>
    {
        CardSnapshot _card;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            _card = table.getRecordCardSnapshot(record, Arg.VAR_CARD);
            table.updateCardBySnapshot(_card, record);
            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
