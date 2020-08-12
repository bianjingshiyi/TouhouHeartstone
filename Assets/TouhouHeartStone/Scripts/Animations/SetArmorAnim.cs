using TouhouHeartstone;
using UI;
namespace Game
{
    class SetArmorAnim : EventAnimation<THHCard.SetArmorEventArg>
    {
        public override bool update(TableManager table, THHCard.SetArmorEventArg eventArg)
        {
            if (eventArg.card.getOwner() == table.player)
            {
                if (eventArg.value > 0)
                {
                    table.ui.SelfMaster.ArmorImage.display();
                    table.ui.SelfMaster.ArmorPropNumber.display();
                    table.ui.SelfMaster.ArmorPropNumber.asText.text = eventArg.value.ToString();
                }
                else
                    table.ui.SelfMaster.ArmorImage.hide();
            }
            else
            {
                if (eventArg.value > 0)
                {
                    table.ui.EnemyMaster.ArmorImage.display();
                    table.ui.EnemyMaster.ArmorPropNumber.display();
                    table.ui.EnemyMaster.ArmorPropNumber.asText.text = eventArg.value.ToString();
                }
                else
                    table.ui.EnemyMaster.ArmorImage.hide();
            }
            return true;
        }
    }
}
