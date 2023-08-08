using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.ChooseOneEventArg;

    public class ChooseOneAnim : EventAnimation<Arg>
    {
        public override bool forceQueueAnimations => true;

        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            //自己打出的效果不展示
            if (player == table.player)
            {
                yield break;
            }

            table.setOpponentWaiting(true);

            yield return pauseWhileNotCompleted();

            table.setOpponentWaiting(false);

            yield return stopWhenCanceled();

            var choose = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            var isShow = arg.isShow;
            TableCard chooseCard = null;
            if (isShow)
            {
                chooseCard = table.createInvokeCard(choose, table.table.LeftDisplayTransform.position, -1, true);
                chooseCard.setFaceup(true);
            }
            if (chooseCard)
            {
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, chooseCard.onShowAnim, chooseCard.animator, _deltaTime))
                    yield return null;
                Object.Destroy(chooseCard.gameObject);
            }
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
            {
                return false;
            }
            return base.blockAnim(nextAnim);
        }
    }
}