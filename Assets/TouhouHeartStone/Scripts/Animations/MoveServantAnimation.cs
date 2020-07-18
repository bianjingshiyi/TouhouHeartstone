using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    class MoveServantAnimation : EventAnimation<THHPlayer.MoveEventArg>
    {
        public override bool update(TableManager table, THHPlayer.MoveEventArg eventArg)
        {
            table.createServant(eventArg.card, eventArg.position);
            return true;
        }
    }
}