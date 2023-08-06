using UnityEngine;
using TouhouHeartstone;
using UI;
using Card = UI.Card;
using TouhouCardEngine;
using System.Collections;

namespace Game.BattleTable
{
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
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

            while (!record.isCompleted)
                yield return null;

            table.setOpponentWaiting(false);

            if (record.isCanceled)
            {
                yield break;
            }
            var discovered = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            TableCard discoveredUI = null;
            var isShow = arg.isShow;
            if (isShow)
            {
                discoveredUI = table.createInvokeCard(discovered, table.table.LeftDisplayTransform.position, -1, true);
                discoveredUI.setFaceup(true);
            }
            if (discoveredUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, discoveredUI.onShowAnim, discoveredUI.animator, _deltaTime))
                    yield return null;
                Object.Destroy(discoveredUI.gameObject);
            }
        }
    }

}