using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = THHPlayer.DiscoverEventArg;
    public class DiscoverAnim : EventAnimation<Arg>
    {
        TableAnimation _anim;
        public override bool forceQueueAnimations => true;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            var defines = arg.defines;

            if (player == table.player)
            {
                yield break;
            }

            table.setOpponentWaiting(true);

            yield return pauseWhileNotCompleted();

            table.setOpponentWaiting(false);

            yield return stopWhenCanceled();

            var discovered = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            TableCard discoveredUI = null;
            var isShow = arg.isShow;
            if (isShow)
            {
                var cardPos = table.WorldToTablePosition(table.table.LeftDisplayTransform.position);
                discoveredUI = table.createInvokeCard(discovered, cardPos, -1, true);
                discoveredUI.setFaceup(true);
            }
            if (discoveredUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, discoveredUI.onShowAnim, discoveredUI.animator, _deltaTime))
                    yield return null;
                discoveredUI.destroy();
            }
        }
    }

}