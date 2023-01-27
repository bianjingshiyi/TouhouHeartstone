using TouhouHeartstone;
using BJSYGameCore.UI;
using UI;
namespace Game
{
    class EnterFieldAnim : EventAnimation<HearthStoneRule.EnterFieldEventArg>
    {
        //随从的入场动画
        AnimAnim _anim;
        public override bool update(TableManager table, HearthStoneRule.EnterFieldEventArg eventArg, float time)
        {
            if (eventArg.from != null)
            {
                if (eventArg.from?.name == PILE.HAND)
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        table.ui.SelfHandList.removeItem(hand);//移除手牌
                    }
                }
            }
            if (!table.tryGetServant(eventArg.card, out var servant))
                servant = table.createServant(eventArg.card, eventArg.position, eventArg.attack, eventArg.life);
            if (!SimpleAnimHelper.update(table, ref _anim, servant.onSummon, servant.servantAnimator, time))
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