using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
namespace UI
{
    class TurnStartAnimation : Animation<THHGame.TurnStartEventArg>
    {
        public TurnStartAnimation(THHGame.TurnStartEventArg eventArg) : base(eventArg)
        {
        }
        Timer _timer = new Timer() { duration = 2 };
        public override bool update(Table table)
        {
            if (!_timer.isStarted)
            {
                table.TurnTipImage.display();
                if (eventArg.player == table.player)
                {
                    table.TurnTipText.text = "你的回合";
                    table.canControl = true;
                }
                else
                {
                    table.TurnTipText.text = "对手的回合";
                    table.canControl = false;
                }
                table.TurnTipImage.GetComponent<Animator>().Play("Display");
                foreach (var servant in table.SelfFieldList)
                {
                    servant.update(table.player, servant.card, table.getSkin(servant.card));
                }
                foreach (var servant in table.EnemyFieldList)
                {
                    servant.update(table.game.getOpponent(table.player), servant.card, table.getSkin(servant.card));
                }
                _timer.start();
            }
            if (!_timer.isExpired())
                return false;
            table.TurnTipImage.hide();
            return true;
        }
    }
}
