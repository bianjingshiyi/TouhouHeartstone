using TouhouHeartstone;
using BJSYGameCore;
using System.Linq;
using UI;
namespace Game
{
    class DamageAnimation : EventAnimation<THHCard.DamageEventArg>
    {
        Timer _timer = new Timer() { duration = .2f };
        public override bool update(TableManager table, THHCard.DamageEventArg eventArg)
        {
            if (!_timer.isStarted)
            {
                foreach (var card in eventArg.cards)
                {
                    if (table.tryGetServant(card, out var servant))
                    {
                        servant.DamageImage.display();
                        servant.DamageText.text = "-" + eventArg.value;
                        servant.HpText.text = eventArg.infoDic[card].currentLife.ToString();
                    }
                    else if (table.tryGetMaster(card, out Master master))
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
                if (table.tryGetServant(card, out Servant servant))
                {
                    servant.DamageImage.setAlpha(_timer.progress);
                    servant.DamageText.setAlpha(_timer.progress);
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.DamageImage.setAlpha(_timer.progress);
                    master.DamageText.setAlpha(_timer.progress);
                }
            }
            if (!_timer.isExpired())
                return false;
            foreach (var card in eventArg.cards)
            {
                if (table.tryGetServant(card, out Servant servant))
                {
                    servant.DamageImage.hide();
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.DamageImage.hide();
                }
            }
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is DamageAnimation damage && damage.tEventArg.cards.Intersect(tEventArg.cards).Count() == 0)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}
