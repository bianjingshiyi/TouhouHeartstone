using TouhouHeartstone;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class SetArmorAnim : EventAnimation<THHCard.SetArmorEventArg>
    {
        public override bool update(TableManager table, THHCard.SetArmorEventArg eventArg, float time)
        {
            if (eventArg.card.getOwner() == table.player)
            {
                if (eventArg.value > 0)
                {
                    table.ui.SelfMaster.ArmorPropNumber.display();
                    table.ui.SelfMaster.ArmorPropNumber.text.text = eventArg.value.ToString();
                }
                else
                    table.ui.SelfMaster.ArmorPropNumber.hide();
            }
            else
            {
                if (eventArg.value > 0)
                {
                    table.ui.EnemyMaster.ArmorPropNumber.display();
                    table.ui.EnemyMaster.ArmorPropNumber.text.text = eventArg.value.ToString();
                }
                else
                    table.ui.EnemyMaster.ArmorPropNumber.hide();
            }
            return true;
        }
    }
}
