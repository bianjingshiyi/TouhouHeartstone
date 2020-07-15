using TouhouHeartstone;
using UI;
using System.Linq;
namespace Game
{
    public class GameEndAnimation : EventAnimation<THHGame.GameEndEventArg>
    {
        public override bool update(TableManager table, THHGame.GameEndEventArg eventArg)
        {
            var grd = table.ui.ui.getObject<GameResultDialog>();
            grd.ShowGameResult(eventArg.winners.Contains(table.player));
            return true;
        }
    }
}
