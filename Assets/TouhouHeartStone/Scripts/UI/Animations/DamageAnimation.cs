using TouhouHeartstone;
using BJSYGameCore;
using System.Linq;
namespace UI
{
    class DamageAnimation : Animation<THHCard.DamageEventArg>
    {
        public DamageAnimation(THHCard.DamageEventArg eventArg) : base(eventArg)
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
                        servant.DamageImage.display();
                        servant.DamageText.text = eventArg.value.ToString();
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
                    servant.DamageImage.setAlpha(_timer.progress);
                    servant.DamageText.setAlpha(_timer.progress);
                }
            }
            if (!_timer.isExpired())
                return false;
            foreach (var card in eventArg.cards)
            {
                var target = table.getCharacter(card);
                if (target is Servant servant)
                {
                    servant.DamageImage.hide();
                }
            }
            return true;
        }
        public override bool blockAnim(Animation nextAnim)
        {
            if (nextAnim is DamageAnimation damage && damage.eventArg.cards.Intersect(eventArg.cards).Count() == 0)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}
