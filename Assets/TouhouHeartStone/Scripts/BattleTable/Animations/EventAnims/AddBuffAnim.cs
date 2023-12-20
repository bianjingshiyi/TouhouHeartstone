using System.Collections;
using TouhouCardEngine;

namespace Game.BattleTable
{
    using Arg = AddBuffEventDefine;
    // 用于解决使用增益给卡牌添加遗言后不会立即更新卡牌遗言图标的问题。
    class AddBuffAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, Arg.VAR_CARD);
            table.updateCardBySnapshot(card);
            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
