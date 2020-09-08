using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    class EnterFieldAnim : EventAnimation<THHPlayer.EnterFieldEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, THHPlayer.EnterFieldEventArg eventArg)
        {
            if (!table.tryGetServant(eventArg.card, out var servant))
                servant = table.createServant(eventArg.card, eventArg.position, eventArg.attack, eventArg.life);
            if (!SimpleAnimHelper.update(table, ref _anim, servant.onSummon, servant.animator))
                return false;
            return true;
        }
    }
}