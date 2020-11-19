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
            table.ui.TimeoutSlider.hide();
            table.closeDiscoverDialog();
            table.ui.TurnEndButton.interactable = false;

            if (table.tryGetMaster(eventArg.player.master, out var master))
            {
                table.setMaster(master, eventArg.player.master);
            }

            foreach (TouhouCardEngine.Card s in eventArg.player.field)
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
