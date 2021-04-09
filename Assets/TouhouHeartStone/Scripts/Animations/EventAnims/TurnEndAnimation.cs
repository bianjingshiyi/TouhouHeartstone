using TouhouHeartstone;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class TurnEndAnimation : EventAnimation<THHGame.TurnEndEventArg>
    {
        public override bool update(TableManager table, THHGame.TurnEndEventArg eventArg, float time)
        {
            table.canControl = false;
            table.cancelAttack();
            table.ui.TimeoutSlider.hide();
            table.closeDiscoverDialog();
            table.ui.TurnEndButton.interactable = false;

            if (table.tryGetMaster(eventArg.player.getMaster(), out var master))
            {
                table.setMaster(master, eventArg.player.getMaster());
            }

            foreach (TouhouCardEngine.Card s in eventArg.player.getField())
            {
                if (table.tryGetServant(s, out var servant))
                {
                    table.setServant(servant, s);
                }
            }
            return true;
        }
    }
}
