using TouhouHeartstone;
using BJSYGameCore;
using System.Linq;
using UI;
using System.Collections.Generic;
namespace Game
{
    class DamageAnimation : EventAnimation<THHCard.DamageEventArg>
    {
        List<AnimAnim> _animList = null;
        Timer _timer = new Timer() { duration = .2f };
        public override bool update(TableManager table, THHCard.DamageEventArg eventArg)
        {
            if (!_timer.isStarted)
            {
                _timer.start();
            }
            if (_animList == null)
            {
                _animList = new List<AnimAnim>();
                foreach (var card in eventArg.infoDic.Keys)
                {
                    if (table.tryGetServant(card, out var servant))
                    {
                        servant.DamageImage.display();
                        servant.DamageText.text = "-" + eventArg.value;
                        servant.HpTextPropNumber.asText.text = eventArg.infoDic[card].currentLife.ToString();
                        servant.onDamage.beforeAnim.Invoke();
                        _animList.Add(new AnimAnim(servant.animator, servant.onDamage.animName));
                    }
                    else if (table.tryGetMaster(card, out Master master))
                    {
                        master.DamageImage.display();
                        master.DamageText.text = "-" + eventArg.value;
                        master.LifePropNumber.asText.text = eventArg.infoDic[card].currentLife.ToString();
                        master.onDamage.beforeAnim.Invoke();
                        _animList.Add(new AnimAnim(master.animator, master.onDamage.animName));
                    }
                }
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
            bool isAllAnimDone = true;
            foreach (var anim in _animList)
            {
                if (!anim.update(table))
                    isAllAnimDone = false;
            }
            if (!isAllAnimDone || !_timer.isExpired())
                return false;
            foreach (var card in eventArg.infoDic.Keys)
            {
                if (table.tryGetServant(card, out var servant))
                {
                    servant.onDamage.afterAnim.Invoke();
                    servant.DamageImage.hide();
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.onDamage.afterAnim.Invoke();
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
