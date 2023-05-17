using TouhouHeartstone;
using UI;
using TouhouCardEngine;
using System.Collections;

namespace Game
{
    using Arg = HearthStoneRule.FatigueEventArg;
    class FatigueAnimation : EventAnimation<Arg>
    {
        AnimAnim _anim;

        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            if (record.isCanceled)
                yield break;

            var player = arg.player;
            var damage = arg.damage;

            table.ui.Fatigue.Text.text = "没有牌了，你受到了" + damage + "点伤害！";
            var deck = player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck;
            table.ui.Fatigue.rectTransform.position = deck.rectTransform.position;
            table.ui.Fatigue.display();
            _anim = new AnimAnim(table.ui.Fatigue.animator, deck.onFatigue.animName);
            while (!_anim.update(table, _time))
                yield return null;
            #region 注释代码
            //table.ui.Fatigue.rectTransform.localScale = Vector3.Lerp(Vector3.one * .5f, Vector3.one, table.ui.Fatigue.moveCurve.Evaluate(_timer.getProgress(0, .5f)));
            //if (eventArg.player == table.player)
            //    table.ui.Fatigue.rectTransform.localPosition = Vector3.Lerp(table.ui.SelfDeck.rectTransform.localPosition, Vector3.zero, table.ui.Fatigue.moveCurve.Evaluate(_timer.getProgress(0, .5f)));
            //else
            //    table.ui.Fatigue.rectTransform.localPosition = Vector3.Lerp(table.ui.EnemyDeck.rectTransform.localPosition, Vector3.zero, table.ui.Fatigue.moveCurve.Evaluate(_timer.getProgress(0, .5f)));
            //table.ui.Fatigue.alpha = 1 - table.ui.Fatigue.fadeCurve.Evaluate(_timer.getProgress(1, 1.5f));
            //if (!_timer.isExpired())
            //    return false;
            //table.ui.Fatigue.hide();
            #endregion
            table.ui.Fatigue.hide();
        }
    }
}
