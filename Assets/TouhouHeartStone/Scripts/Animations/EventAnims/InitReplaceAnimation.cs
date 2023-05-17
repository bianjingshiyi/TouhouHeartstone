using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
namespace Game
{
    using Arg = HearthStoneRule.InitReplaceEventArg;
    class InitReplaceAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            table.ui.InitReplaceDialog.hide();
            yield break;
        }
    }
}
