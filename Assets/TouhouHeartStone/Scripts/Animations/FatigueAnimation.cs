using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
using UI;
namespace Game
{
    class FatigueAnimation : EventAnimation<THHPlayer.FatigueEventArg>
    {
        Timer _timer = new Timer() { duration = 1.5f };
        public override bool update(TableManager table, THHPlayer.FatigueEventArg eventArg)
        {
            if (!_timer.isStarted)
            {
                table.ui.Fatigue.display();
                table.ui.Fatigue.Text.text = "没有牌了，你受到了" + eventArg.player.fatigue + "点伤害！";
                _timer.start();
            }
            table.ui.Fatigue.rectTransform.localScale = Vector3.Lerp(Vector3.one * .5f, Vector3.one, table.ui.Fatigue.moveCurve.Evaluate(_timer.getProgress(0, .5f)));
            if (eventArg.player == table.player)
                table.ui.Fatigue.rectTransform.localPosition = Vector3.Lerp(table.ui.SelfDeck.rectTransform.localPosition, Vector3.zero, table.ui.Fatigue.moveCurve.Evaluate(_timer.getProgress(0, .5f)));
            else
                table.ui.Fatigue.rectTransform.localPosition = Vector3.Lerp(table.ui.EnemyDeck.rectTransform.localPosition, Vector3.zero, table.ui.Fatigue.moveCurve.Evaluate(_timer.getProgress(0, .5f)));
            table.ui.Fatigue.alpha = 1 - table.ui.Fatigue.fadeCurve.Evaluate(_timer.getProgress(1, 1.5f));
            if (!_timer.isExpired())
                return false;
            table.ui.Fatigue.hide();
            return true;
        }
    }
}
