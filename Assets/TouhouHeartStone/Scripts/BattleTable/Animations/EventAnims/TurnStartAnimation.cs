using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using Timer = BJSYGameCore.Timer;

namespace Game.BattleTable
{
    using Arg = TurnStartEventDefine;
    class TurnStartAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);

            var timer = new Timer() { duration = 2 };
            if (!timer.isStarted)
            {
                timer.start();
                table.showTurnTipImage(player == table.player);
                table.currentAnimPlayer = player;
                if (player == table.player)
                {
                    table.startLocalTurn();
                }
                updateCards(table);
            }
            while (!timer.isExpired())
                yield return null;
            table.hideTurnTipImage();
        }

        private void updateCards(BattleTableManager table)
        {
            // 更新道具的闭合。
            foreach (var itemUI in table.getAllItems())
            {
                var item = table.getCardByItem(itemUI);
                if (item == null)
                    continue;
                var snapshot = table.game.getCardSnapshotBeforeEvent(item, eventArg);
                table.setItem(itemUI, snapshot);
            }

            // 更新陷阱的明暗变化。
            foreach (var secretUI in table.getAllSecrets())
            {
                var secret = table.getCardBySecret(secretUI);
                if (secret == null)
                    continue;
                var snapshot = table.game.getCardSnapshotBeforeEvent(secret, eventArg);
                table.setSecret(secretUI, snapshot);
            }
        }
        public override bool continueNextGroup(IEnumerable<TableAnimation> nextGroup)
        {
            if (!nextGroup.Any(a => a is TurnStartAnimation || a is UseAnimation))
                return true;
            return base.continueNextGroup(nextGroup);
        }
    }
}