using System.Collections;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    using Arg = HearthStoneRule.TurnEndEventArg;
    class TurnEndAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            table.canControl = false;
            table.cancelAttack();
            table.ui.TimeoutSlider.hide();
            table.closeDiscoverDialog();
            table.ui.TurnEndButton.interactable = false;
            yield break;
        }
    }
}
