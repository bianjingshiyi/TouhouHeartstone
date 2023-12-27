using System.Collections;
using TouhouCardEngine;

namespace Game.BattleTable
{
    using Arg = ChangeCardVisibilityEventDefine;
    class ChangeCardVisibilityAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, Arg.VAR_CARD);
            var player = eventArg.getVar<Player>(Arg.VAR_PLAYER);
            var visible = eventArg.getVar<bool>(Arg.VAR_VISIBLE);

            if (table.tryGetHand(card, out var handItem))
            {
                while (table.isTurnTipImageActive())
                {
                    yield return null;
                }
                updateHand(table, handItem, player, visible);
            }
            table.updateCardDescriptor(card);
            yield break;
        }
        private void updateHand(BattleTableManager table, HandItem hand, Player player, bool visible)
        {
            if (player == table.player)
            {
                hand.card.animateFaceup(visible);
            }
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
