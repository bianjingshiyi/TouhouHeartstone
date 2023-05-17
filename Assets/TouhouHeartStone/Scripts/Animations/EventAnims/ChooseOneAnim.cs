using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = UI.Card;
namespace Game
{
    using Arg = HearthStoneRule.ChooseOneEventArg;
    public class ChooseOneAnim : EventAnimation<Arg>
    {
        public override bool forceQueueAnimations => true;

        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            //自己打出的效果不展示
            if (player == table.player)
            {
                yield break;
            }

            table.ui.Waiting.SetActive(true);

            while (!record.isCompleted)
                yield return null;

            table.ui.Waiting.SetActive(false);

            if (record.isCanceled)
            {
                yield break;
            }
            var choose = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            table.ui.Waiting.SetActive(false);

            var isShow = arg.isShow;
            Card chooseCard = null;
            if (isShow)
            {
                chooseCard = table.createInvokeCard(choose, table.ui.LeftDisplayImage.rectTransform.position, table.cardPrefab.rectTransform.rect.height * table.ui.SelfHandList.rectTransform.localScale.y * 1.2f, isShow);
                chooseCard.asAnimator.SetBool("isFaceUp", isShow);
            }
            if (chooseCard)
            {
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, chooseCard.ShowAnim, chooseCard.animator, _time))
                    yield return null;
                Object.Destroy(chooseCard.gameObject);
            }
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
            {
                return false;
            }
            return base.blockAnim(nextAnim);
        }
    }

}