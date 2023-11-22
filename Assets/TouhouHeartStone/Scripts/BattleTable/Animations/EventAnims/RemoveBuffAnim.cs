using System.Collections;
using TouhouCardEngine;

namespace Game.BattleTable
{
    using Arg = Card.RemoveBuffEventArg;
    class RemoveBuffAnim : EventAnimation<Arg>
    {
        CardSnapshot _card;
        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
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
