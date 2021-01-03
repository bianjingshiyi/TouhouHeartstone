using TouhouHeartstone;
using BJSYGameCore;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class HealAnimation : EventAnimation<THHCard.HealEventArg>
    {
        Timer _timer = new Timer() { duration = .2f };
        public override bool update(TableManager table, THHCard.HealEventArg eventArg, float time)
        {
            if (!_timer.isStarted)
            {
                foreach (var card in eventArg.cards)
                {
                    if (table.tryGetServant(card,out ServantItem servant))
                    {
                        servant.healImage.display();
                        servant.healText.text = "+" + eventArg.infoDic[card].healedValue.ToString();
                        table.setServant(table.getServant(card), card);
                    }
                }
                _timer.start();
            }
            foreach (var card in eventArg.cards)
            {
                if (table.tryGetServant(card, out ServantItem servant))
                {
                    servant.healImage.setAlpha(_timer.progress);
                    servant.healText.setAlpha(_timer.progress);
                }
            }
            if (!_timer.isExpired())
                return false;
            foreach (var card in eventArg.cards)
            {
                if (table.tryGetServant(card, out ServantItem servant))
                {
                    servant.healImage.hide();
                }
            }
            return true;
        }
    }
}
