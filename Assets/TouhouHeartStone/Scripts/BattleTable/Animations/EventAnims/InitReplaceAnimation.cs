using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game.BattleTable
{
    using Arg = InitReplaceEventDefine;
    class InitReplaceAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            table.ui.StartDialog.setActive(false);
            table.completeMulligan();
            yield break;
        }
    }
}
