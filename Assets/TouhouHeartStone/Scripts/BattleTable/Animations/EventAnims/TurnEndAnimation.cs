using System.Collections;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.TurnEndEventArg;
    class TurnEndAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            table.canControl = false;
            table.cancelAttack();
            table.closeDiscoverDialog();
            table.table.TurnEndButton.setInteractable(false);
            yield break;
        }
    }
}
