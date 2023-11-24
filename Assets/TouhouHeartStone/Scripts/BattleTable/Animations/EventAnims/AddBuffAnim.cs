using System.Collections;
using TouhouCardEngine;

namespace Game.BattleTable
{
    using Arg = Card.AddBuffEventArg;
    class AddBuffAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(record, Arg.VAR_CARD);
            table.updateCardBySnapshot(card);
            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
