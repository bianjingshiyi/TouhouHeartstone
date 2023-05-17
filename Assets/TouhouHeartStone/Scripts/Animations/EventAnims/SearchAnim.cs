using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = UI.Card;

namespace Game
{
    using Arg = HearthStoneRule.SearchPileEventArg;
    public class SearchAnim : EventAnimation<Arg>
    {
        TableAnimation _anim;
        public override bool forceQueueAnimations => true;
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var eventArg = record.eventArg as Arg;
            var player = eventArg.player;
            var cards = eventArg.cards;
            //自己打出的效果不展示
            if (player == table.player)
                yield break;

            table.ui.Waiting.SetActive(true);

            table.ui.Waiting.SetActive(true);

            while (!record.isCompleted)
                yield return null;

            table.ui.Waiting.SetActive(false);

            if (record.isCanceled)
            {
                yield break;
            }
            var searched = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            Card searchedUI = null;
            var isShow = arg.isShow;
            if (isShow)
            {
                searchedUI = table.createInvokeCard(searched, table.ui.LeftDisplayImage.rectTransform.position, table.cardPrefab.rectTransform.rect.height * table.ui.SelfHandList.rectTransform.localScale.y * 1.2f, isShow);
                searchedUI.asAnimator.SetBool("isFaceUp", isShow);
            }
            if (searchedUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, searchedUI.ShowAnim, searchedUI.animator, _time))
                    yield return null;
                Object.Destroy(searchedUI.gameObject);
            }
        }
    }
}