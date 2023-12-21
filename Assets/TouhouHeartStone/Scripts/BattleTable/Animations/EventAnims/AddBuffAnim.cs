using System.Collections;
using Game.Animations;
using TouhouCardEngine;
using UI;

namespace Game.BattleTable
{
    using Arg = AddBuffEventDefine;
    // 用于解决使用增益给卡牌添加遗言后不会立即更新卡牌遗言图标的问题。
    class AddBuffAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, Arg.VAR_CARD);
            var buff = eventArg.getVar<Buff>(Arg.VAR_BUFF);
            table.updateCardBySnapshot(card);

            if (buff is GeneratedBuff gen)
            {
                var defRef = gen.defineRef;
                var buffSkin = table.getBuffSkin(defRef);
                DefineReference animRef = buffSkin?.persistentAnimRef;
                if (animRef != null)
                {
                    var srcTransform = table.getCardTransform(card.id, true);
                    var info = new VisualEffectInfo()
                    {
                        srcTransform = srcTransform,
                        destTransform = srcTransform,
                    };
                    var effect = table.createEffect(animRef, info);
                    table.addCardBuffEffect(card.id, buff.instanceID, effect);
                }
            }

            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
