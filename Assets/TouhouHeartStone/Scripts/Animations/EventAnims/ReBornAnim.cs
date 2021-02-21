using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    public class ReBornAnim : EventAnimation<THHPlayer.RebornEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, THHPlayer.RebornEventArg eventArg, float time)
        {
            if (!table.tryGetServant(eventArg.card, out var servant))
                servant = table.createServant(eventArg.card, eventArg.position, eventArg.card.getAttack(table.game), eventArg.card.getLife(table.game));
            if (!SimpleAnimHelper.update(table, ref _anim, servant.onSummon, servant.servantAnimator, time))
                return false;
            return true;
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is ReBornAnim reBorn && reBorn.tEventArg.card != tEventArg.card)
                return false;
            return base.blockAnim(nextAnim);
        }
    }


}
