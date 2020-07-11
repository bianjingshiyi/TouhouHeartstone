using TouhouHeartstone;
using UI;
namespace Game
{
    class GameEndAnimation : EventAnimation<THHGame.GameEndEventArg>
    {
        public override bool update(TableManager table, THHGame.GameEndEventArg eventArg)
        {
            return true;
        }
    }
}
