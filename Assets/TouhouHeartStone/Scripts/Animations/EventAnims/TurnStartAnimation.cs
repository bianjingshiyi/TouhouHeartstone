using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class TurnStartAnimation : EventAnimation<THHGame.TurnStartEventArg>
    {
        Timer _timer = new Timer() { duration = 2 };
        public override bool update(TableManager table, THHGame.TurnStartEventArg eventArg, float time)
        {

            if (!_timer.isStarted)
            {
                _timer.start();
                table.ui.TurnTipImage.display();
                if (eventArg.player == table.player)
                {
                    table.ui.SetTurnTipText(true);
                    table.canControl = true;
                }
                else
                {
                    table.ui.SetTurnTipText(false);
                    table.canControl = false;
                }
                table.ui.TurnTipImage.GetComponent<Animator>().Play("Display");
                foreach (var player in table.game.players)
                {
                    table.setSkill(player == table.player ? table.ui.SelfSkill : table.ui.EnemySkill, player.getSkill());
                    foreach (var card in player.getField())
                    {
                        var servant = table.getServant(card);
                        table.setServant(servant, card);
                    }
                    table.setMaster(player == table.player ? table.ui.SelfMaster: table.ui.EnemyMaster, player.getMaster());
                }
            }
            if (!_timer.isExpired())
                return false;
            table.ui.TurnTipImage.hide();
            table.ui.TurnEndButton.interactable = eventArg.player == table.player;

            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            return false;
        }
    }
}