using TouhouHeartstone;
using UI;
namespace Game
{
    class TurnEndAnimation : EventAnimation<THHGame.TurnEndEventArg>
    {
        public override bool update(TableManager table, THHGame.TurnEndEventArg eventArg)
        {
            table.canControl = false;
            return true;
        }
    }
}
