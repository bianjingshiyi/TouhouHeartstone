using TouhouHeartstone;
using BJSYGameCore;
namespace UI
{
    class HealAnimation : Animation<THHCard.HealEventArg>
    {
        public HealAnimation(THHCard.HealEventArg eventArg) : base(eventArg)
        {
        }
        Timer _timer = new Timer() { duration = .2f };
        public override bool update(Table table)
        {
            if (!_timer.isStarted)
            {
                foreach (var card in eventArg.cards)
                {
                    var target = table.getCharacter(card);
                    if (target is Servant servant)
                    {
                        servant.HealImage.display();
                        servant.HealText.text = "+" + eventArg.infoDic[card].healedValue.ToString();
                        servant.update(card.owner as THHPlayer, card, table.getSkin(card));
                    }
                }
                _timer.start();
            }
            foreach (var card in eventArg.cards)
            {
                var target = table.getCharacter(card);
                if (target is Servant servant)
                {
                    servant.HealImage.setAlpha(_timer.progress);
                    servant.HealText.setAlpha(_timer.progress);
                }
            }
            if (!_timer.isExpired())
                return false;
            foreach (var card in eventArg.cards)
            {
                var target = table.getCharacter(card);
                if (target is Servant servant)
                {
                    servant.HealImage.hide();
                }
            }
            return true;
        }
    }
}
