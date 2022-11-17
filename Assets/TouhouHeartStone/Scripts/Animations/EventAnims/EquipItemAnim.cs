using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    class EquipItemAnim : EventAnimation<HearthStoneRule.EquipItemEventArg>
    {
        //随从的入场动画
        AnimAnim _anim;
        public override bool update(TableManager table, HearthStoneRule.EquipItemEventArg eventArg, float time)
        {
            if (eventArg.from != null)
            {
                if (eventArg.from?.name == PILE.HAND)
                {
                    table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));//移除手牌
                }
            }
            table.setMaster(table.ui.SelfMaster, table.player.getMaster());
            if (!table.tryGetItem(eventArg.card, out var item))
                return false;

            item.AttackPropNumber.text.text = eventArg.card.getAttack(table.game).ToString();
            item.DurabilityPropNumber.text.text = eventArg.card.getCurrentLife(table.game).ToString();
            table.setItem(item, eventArg.card);

            if (!SimpleAnimHelper.update(table, ref _anim, item.onEquip, item.animator, time))
                return false;
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is EquipItemAnim enter && enter.tEventArg.card != tEventArg.card)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}