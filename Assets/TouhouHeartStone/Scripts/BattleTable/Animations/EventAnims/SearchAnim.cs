using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = UI.Card;

namespace Game.BattleTable
{
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
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

            while (!record.isCompleted)
                yield return null;

            table.setOpponentWaiting(false);

            if (record.isCanceled)
            {
                yield break;
            }
            var searched = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            TableCard searchedUI = null;
            var isShow = arg.isShow;
            if (isShow)
            {
                searchedUI = table.createInvokeCard(searched, table.table.LeftDisplayTransform.position, -1, true);
                searchedUI.animateFaceup(true);
            }
            if (searchedUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, searchedUI.onShowAnim, searchedUI.animator, _time))
                    yield return null;
                Object.Destroy(searchedUI.gameObject);
            }
        }
    }
}