using System.Collections;
using Game.Animations;
using TouhouCardEngine;
using TouhouHeartstone;
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

            var buffDefine = buff.define;
            var buffSkin = table.getBuffSkin(buffDefine);

            var persAnimRef = buffSkin?.persistentAnimRef;
            DefineReference animRef = persAnimRef?.animRef;
            if (animRef != null && !table.hasBuffEffect(card.id, animRef))
            {
                var srcTransform = table.getCardTransform(card.id, true);
                var info = new VisualEffectInfo()
                {
                    srcTransform = srcTransform,
                    destTransform = srcTransform,
                };
                bool visible = true;
                if (buff.isHiddenToVisible() && !buff.isPropInvisibleTo(BUFF.HIDDEN_TO_VISIBLE, table.player))
                {
                    visible = false;
                }
                var effect = table.createEffect(animRef, info);
                if (effect)
                {
                    effect.modifyColors(persAnimRef.colorModifies);

                    effect.setVisible(visible);
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
