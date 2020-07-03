using TouhouHeartstone;
using BJSYGameCore;
using UI;
namespace Game
{
    class HealAnimation : EventAnimation<THHCard.HealEventArg>
    {
        Timer _timer = new Timer() { duration = .2f };
        public override bool update(TableManager table, THHCard.HealEventArg eventArg)
        {
            if (!_timer.isStarted)
            {
                foreach (var card in eventArg.cards)
                {
                    if (table.tryGetServant(card,out Servant servant))
                    {
                        servant.HealImage.display();
                        servant.HealText.text = "+" + eventArg.infoDic[card].healedValue.ToString();
                        table.setServant(table.getServant(card), card);
                    }
                }
                _timer.start();
            }
            foreach (var card in eventArg.cards)
            {
                if (table.tryGetServant(card, out Servant servant))
                {
                    servant.HealImage.setAlpha(_timer.progress);
                    servant.HealText.setAlpha(_timer.progress);
                }
            }
            if (!_timer.isExpired())
                return false;
            foreach (var card in eventArg.cards)
            {
                if (table.tryGetServant(card, out Servant servant))
                {
                    servant.HealImage.hide();
                }
            }
            return true;
        }
    }
}
