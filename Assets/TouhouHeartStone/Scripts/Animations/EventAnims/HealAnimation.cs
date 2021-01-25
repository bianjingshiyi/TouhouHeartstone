using TouhouHeartstone;
using BJSYGameCore;
using UI;
using BJSYGameCore.UI;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
    class HealAnimation : EventAnimation<THHCard.HealEventArg>
    {
        List<AnimAnim> _animList = null;
        public override bool update(TableManager table, THHCard.HealEventArg eventArg, float time)
        {
            if (_animList == null)
            {
                _animList = new List<AnimAnim>();
                foreach (var card in eventArg.cards)
                {
                    if (table.tryGetServant(card, out ServantItem servant))
                    {
                        servant.healText.text = "+" + eventArg.infoDic[card].healedValue.ToString();
                        servant.onHeal.invokeBeforeAnim();
                        _animList.Add(new AnimAnim(servant.servantAnimator, servant.onHeal.animName));
                    }
                    else if (table.tryGetMaster(card, out Master master))
                    {
                        master.HealText.text = "+" + eventArg.infoDic[card].healedValue.ToString();
                        master.onHeal.invokeBeforeAnim();
                        _animList.Add(new AnimAnim(master.animator, master.onHeal.animName));
                    }
                }
                #region 控制透明度
                //foreach (var card in eventArg.cards)
                //{
                //    if (table.tryGetServant(card, out ServantItem servant))
                //    {
                //        servant.healImage.setAlpha(_timer.progress);
                //        servant.healText.setAlpha(_timer.progress);
                //    }
                //}

                //foreach (var card in eventArg.cards)
                //{
                //    if (table.tryGetServant(card, out ServantItem servant))
                //    {
                //        servant.healImage.hide();
                //    }
                //}
                ///
                #endregion
            }
            bool isAllAnimDone = true;
            foreach (var anim in _animList)
            {
                if (!anim.update(table, time))
                    isAllAnimDone = false;
            }
            if (!isAllAnimDone)
                return false;
            foreach (var card in eventArg.infoDic.Keys)
            {
                if (table.tryGetServant(card, out var servant))
                {
                    servant.onHeal.invokeAfterAnim();
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.onHeal.invokeAfterAnim();
                }
            }
            return true;
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
                return true;
            if (nextAnim is TurnEndAnimation)
                return true;
            return false;
        }

    }
}
