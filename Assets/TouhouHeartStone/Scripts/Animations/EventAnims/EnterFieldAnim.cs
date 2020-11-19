using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    class EnterFieldAnim : EventAnimation<THHPlayer.EnterFieldEventArg>
    {
        //随从的入场动画
        AnimAnim _anim;
        public override bool update(TableManager table, THHPlayer.EnterFieldEventArg eventArg, float time)
        {
            if (eventArg.from != null)
            {
                if (eventArg.from.name == PILE.HAND)
                {
                    table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));//移除手牌
                }
            }
            if (!table.tryGetServant(eventArg.card, out var servant))
                servant = table.createServant(eventArg.card, eventArg.position, eventArg.attack, eventArg.life);
            if (!SimpleAnimHelper.update(table, ref _anim, servant.onSummon, servant.animator, time))
                return false;
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is EnterFieldAnim enter && enter.tEventArg.card != tEventArg.card)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}