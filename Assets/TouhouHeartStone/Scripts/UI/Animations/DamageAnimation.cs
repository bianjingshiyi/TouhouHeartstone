using TouhouHeartstone;
using BJSYGameCore;
using System.Linq;
namespace UI
{
    class DamageAnimation : UIAnimation<THHCard.DamageEventArg>
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
                        servant.DamageText.text = "-" + eventArg.value;
                        servant.HpText.text = eventArg.infoDic[card].currentLife.ToString();
                    }
                    else if (target is Master master)
                    {
                        master.DamageImage.display();
                        master.DamageText.text = "-" + eventArg.value;
                        master.HpText.text = eventArg.infoDic[card].currentLife.ToString();
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
                else if (target is Master master)
                {
                    master.DamageImage.setAlpha(_timer.progress);
                    master.DamageText.setAlpha(_timer.progress);
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
                else if (target is Master master)
                {
                    master.DamageImage.hide();
                }
            }
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is DamageAnimation damage && damage.eventArg.cards.Intersect(eventArg.cards).Count() == 0)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}
