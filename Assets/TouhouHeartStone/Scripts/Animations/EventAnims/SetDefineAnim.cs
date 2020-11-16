using UI;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;

namespace Game
{
    class SetDefineAnim : EventAnimation<Card.SetDefineEventArg>
    {
        TableAnimation _anim;
        public override bool update(TableManager table, Card.SetDefineEventArg eventArg, float time)
        {
            Card card = eventArg.card;
            if (table.tryGetServant(card, out var servant))
            {
                if (!SimpleAnimHelper.update(table, ref _anim, servant.onSetDefine, servant.animator, time))
                    return false;
                table.setServant(servant, card);
            }
            else if (table.tryGetMaster(card, out var master))
            {
                if (!SimpleAnimHelper.update(table, ref _anim, master.onSetDefine, master.animator, time))
                    return false;
                table.setMaster(master, card);
            }
            else if (table.tryGetItem(card, out var item))
            {
                if (!SimpleAnimHelper.update(table, ref _anim, item.onSetDefine, item.animator, time))
                    return false;
                table.setItem(item, card);
            }
            else if (table.tryGetSkill(card, out var skill))
            {
                if (!SimpleAnimHelper.update(table, ref _anim, skill.onSetDefine, skill.animator, time))
                    return false;
                table.setSkill(skill, card);
            }
            else if (table.tryGetHand(card, out var hand))
            {
                if (!SimpleAnimHelper.update(table, ref _anim, hand.onSetDefine, hand.animator, time))
                    return false;
                if (card.getOwner() == table.player)
                    table.setCard(hand.Card, card, true);
            }
            return true;
        }
    }
}
