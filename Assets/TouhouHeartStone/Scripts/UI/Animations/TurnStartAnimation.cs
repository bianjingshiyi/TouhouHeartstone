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
                    table.TurnTipText.text = "你的回合";
                else
                    table.TurnTipText.text = "对手的回合";
                table.TurnTipImage.GetComponent<Animator>().Play("Display");
                _timer.start();
            }
            if (!_timer.isExpired())
                return false;
            table.TurnTipImage.hide();
            return true;
        }
    }
}
