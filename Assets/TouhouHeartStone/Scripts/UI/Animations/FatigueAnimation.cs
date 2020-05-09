using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
namespace UI
{
    class FatigueAnimation : Animation<THHPlayer.FatigueEventArg>
    {
        public FatigueAnimation(THHPlayer.FatigueEventArg eventArg) : base(eventArg)
        {
        }
        Timer _timer = new Timer() { duration = 1.5f };
        public override bool update(Table table)
        {
            if (!_timer.isStarted)
            {
                table.Fatigue.display();
                table.Fatigue.Text.text = "没有牌了，你受到了" + eventArg.player.fatigue + "点伤害！";
                _timer.start();
            }
            table.Fatigue.rectTransform.localScale = Vector3.Lerp(Vector3.one * .5f, Vector3.one, _timer.getProgress(0, .5f));
            table.Fatigue.rectTransform.localPosition = Vector3.Lerp(table.SelfDeck.rectTransform.localPosition, Vector3.zero, _timer.getProgress(0, .5f));
            table.Fatigue.alpha = 1 - _timer.getProgress(1, 1.5f);
            table.Fatigue.Text.setAlpha(1 - _timer.getProgress(1, 1.5f));
            if (!_timer.isExpired())
                return false;
            table.Fatigue.hide();
            return true;
        }
    }
}
