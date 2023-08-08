using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.SearchPileEventArg;
    public class SearchAnim : EventAnimation<Arg>
    {
        TableAnimation _anim;
        public override bool forceQueueAnimations => true;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var eventArg = record.eventArg as Arg;
            var player = eventArg.player;
            var cards = eventArg.cards;
            //自己打出的效果不展示
            if (player == table.player)
                yield break;

            table.setOpponentWaiting(true);

            yield return pauseWhileNotCompleted();

            table.setOpponentWaiting(false);

            yield return stopWhenCanceled();

            var searched = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            TableCard searchedUI = null;
            var isShow = arg.isShow;
            if (isShow)
            {
                searchedUI = table.createInvokeCard(searched, table.table.LeftDisplayTransform.position, -1, true);
                searchedUI.setFaceup(true);
            }
            if (searchedUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, searchedUI.onShowAnim, searchedUI.animator, _deltaTime))
                    yield return null;
                Object.Destroy(searchedUI.gameObject);
            }
        }
    }
}