using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game.BattleTable
{
    using Arg = HearthStoneRule.InitReplaceEventArg;
    class InitReplaceAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            table.ui.InitReplaceDialog.setActive(false);
            yield break;
        }
    }
}
