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
                    if (table.tryGetHand(eventArg.card, out var handItem)) 
                    {
                        table.ui.SelfHandList.removeItem(handItem);//移除手牌
                        table.ui.EnemyHandList.removeItem(handItem);//移除手牌
                    }
                }
            }
            table.setMaster(table.ui.SelfMaster, table.player.getMaster());
            var item = eventArg.player == table.player ? table.ui.SelfItem : table.ui.EnemyItem;
            table.setItem(item, eventArg.card);
            item.AttackPropNumber.text.text = eventArg.card.getAttack(table.game).ToString();
            item.DurabilityPropNumber.text.text = eventArg.card.getCurrentLife(table.game).ToString();

            if (!SimpleAnimHelper.update(table, ref _anim, item.onEquip, item.animator, time))
                return false;
            return true;
        }
    }
}