using System.Collections;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
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
                updateMastersAndItems(table);
            }
            while (!timer.isExpired())
                yield return null;
            table.hideTurnTipImage();
            table.table.TurnEndButton.setInteractable(player == table.player);
        }

        private void updateMastersAndItems(BattleTableManager table)
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