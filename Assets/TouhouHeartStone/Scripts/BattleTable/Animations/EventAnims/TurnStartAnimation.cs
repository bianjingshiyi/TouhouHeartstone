using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using Timer = BJSYGameCore.Timer;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.TurnStartEventArg;
    class TurnStartAnimation : EventAnimation<Arg>
    {
        CardSnapshot lastMasterSnapshot;
        CardSnapshot lastItemSnapshot;
        CardSnapshot masterSnapshot;
        CardSnapshot itemSnapshot;
        List<CardSnapshot> secretSnapshots = new List<CardSnapshot>();
        public override void init(BattleTableManager table, Arg eventArg)
        {
            base.init(table, eventArg);

            var player = eventArg.player;
            masterSnapshot = table.game.snapshotCard(player.getMaster());
            var item = player.getItem();
            if (item != null)
                itemSnapshot = table.game.snapshotCard(item);


            var lastPlayer = table.game.getOpponent(eventArg.player);
            lastMasterSnapshot = table.game.snapshotCard(lastPlayer.getMaster());
            var lastItem = lastPlayer.getItem();
            if (lastItem != null)
                lastItemSnapshot = table.game.snapshotCard(lastItem);

            foreach (var p in table.game.players)
            {
                var head = p.getHead();
                foreach (var card in head)
                {
                    secretSnapshots.Add(table.game.snapshotCard(card));
                }
            }
        }
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;

            var timer = new Timer() { duration = 2 };
            if (!timer.isStarted)
            {
                timer.start();
                table.showTurnTipImage(player == table.player);
                table.currentAnimPlayer = player;
                table.canControl = player == table.player;
                table.setAllCardControlDirty();
                updateCards(table);
            }
            while (!timer.isExpired())
                yield return null;
            table.hideTurnTipImage();
            table.table.TurnEndButton.setInteractable(player == table.player);
        }

        private void updateCards(BattleTableManager table)
        {
            if (masterSnapshot != null && table.tryGetMaster(masterSnapshot, out Master master))
            {
                table.setMaster(master, masterSnapshot);
            }
            if (itemSnapshot != null && table.tryGetItem(itemSnapshot, out Item item))
            {
                table.setItem(item, itemSnapshot);
            }

            if (lastMasterSnapshot != null && table.tryGetMaster(lastMasterSnapshot, out Master lastMaster))
            {
                table.setMaster(lastMaster, lastMasterSnapshot);
            }
            if (lastItemSnapshot != null && table.tryGetItem(lastItemSnapshot, out Item lastItem))
            {
                table.setItem(lastItem, lastItemSnapshot);
            }

            foreach (var secret in secretSnapshots)
            {
                if (table.tryGetSecret(secret, out var secretUI))
                {
                    table.setSecret(secretUI, secret);
                }
            }
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is TurnStartAnimation)
                return true;
            else if (!(nextAnim is UseAnimation))
                return false;
            else
                return base.blockAnim(nextAnim);
        }
    }
}