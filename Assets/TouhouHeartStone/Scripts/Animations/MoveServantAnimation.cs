using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    class MoveServantAnimation : EventAnimation<THHPlayer.MoveEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, THHPlayer.MoveEventArg eventArg)
        {
            var servant = table.createServant(eventArg.card, eventArg.position);
            if (!SimpleAnimHelper.update(table, ref _anim, servant.onSummon, servant.animator))
                return false;
            return true;
        }
    }
}