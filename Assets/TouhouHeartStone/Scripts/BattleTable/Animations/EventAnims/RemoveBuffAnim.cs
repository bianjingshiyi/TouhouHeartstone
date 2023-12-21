using System.Collections;
using Game.Animations;
using TouhouCardEngine;

namespace Game.BattleTable
{
    using Arg = RemoveBuffEventDefine;
    // 用于解决使用增益给卡牌添加遗言后不会立即更新卡牌遗言图标的问题。
    class RemoveBuffAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, Arg.VAR_CARD);
            var buff = eventArg.getVar<Buff>(Arg.VAR_BUFF);
            table.updateCardBySnapshot(card);

            if (table.tryGetCardBuffEffect(card.id, buff.instanceID, out TableEffect effect))
            {
                effect.end();
            }

            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
