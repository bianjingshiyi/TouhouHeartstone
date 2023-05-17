using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    using Arg = THHCard.HealEventArg;
    class HealAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            if (record == null || record.isCanceled)
                yield break;
            var cards = arg.cards;
            var source = arg.source;
            var value = arg.value;
            var infoDic = arg.infoDic;


            var animList = new List<AnimAnim>();
            foreach (var card in cards)
            {
                if (table.tryGetServant(card, out ServantItem servant))
                {
                    servant.healText.text = "+" + infoDic[card].healedValue.ToString();
                    servant.onHeal.invokeBeforeAnim();
                    animList.Add(new AnimAnim(servant.servantAnimator, servant.onHeal.animName));
                }
                else if (table.tryGetMaster(card, out Master master))
                {
                    master.HealText.text = "+" + infoDic[card].healedValue.ToString();
                    master.onHeal.invokeBeforeAnim();
                    animList.Add(new AnimAnim(master.animator, master.onHeal.animName));
                }
            }
            #region 控制透明度
            //foreach (var card in cards)
            //{
            //    if (table.tryGetServant(card, out ServantItem servant))
            //    {
            //        servant.healImage.setAlpha(_timer.progress);
            //        servant.healText.setAlpha(_timer.progress);
            //    }
            //}

            //foreach (var card in cards)
            //{
            //    if (table.tryGetServant(card, out ServantItem servant))
            //    {
            //        servant.healImage.hide();
            //    }
            //}
            ///
            #endregion
            while (true)
            {
                bool isAllAnimDone = true;
                foreach (var anim in animList)
                {
                    if (!anim.update(table, _time))
                        isAllAnimDone = false;
                }
                if (isAllAnimDone)
                    break;
                yield return null;
            }
            foreach (var card in infoDic.Keys)
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
