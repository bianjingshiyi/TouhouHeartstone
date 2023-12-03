using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.SelectCardEventArg;
    public class SearchAnim : EventAnimation<Arg>
    {
        TableAnimation _anim;
        public override bool forceQueueAnimations => true;
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = tEventArg.getVar<Player>(EVENTVAR.PLAYER);
            //自己打出的效果不展示
            if (player == table.player)
                yield break;

            table.setOpponentWaiting(true);

            yield return pauseWhileNotCompleted();

            table.setOpponentWaiting(false);

            yield return stopWhenCanceled();

            var searched = table.getEventCardSnapshot(tEventArg, EVENTVAR.CHOSEN_CARD);

            TableCard searchedUI = null;
            var isShow = tEventArg.getVar<bool>(Arg.VAR_CARD_SEEN);
            if (isShow)
            {
                var tablePos = table.WorldToTablePosition(table.table.LeftDisplayTransform.position);
                searchedUI = table.createInvokeCard(searched, tablePos, -1, true);
                searchedUI.setFaceup(true);
            }
            if (searchedUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, searchedUI.onShowAnim, searchedUI.animator, _deltaTime))
                    yield return null;
                searchedUI.destroy();
            }
        }
    }
}