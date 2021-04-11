using TouhouHeartstone;
using UI;
using System.Linq;
namespace Game
{
    public class GameEndAnimation : EventAnimation<HearthStoneRule.GameEndEventArg>
    {
        public override bool update(TableManager table, HearthStoneRule.GameEndEventArg eventArg, float time)
        {
            var grd = table.ui.ui.getObject<GameResultDialog>();
            grd.ShowGameResult(eventArg.winners.Contains(table.player));
            return true;
        }
    }
}
