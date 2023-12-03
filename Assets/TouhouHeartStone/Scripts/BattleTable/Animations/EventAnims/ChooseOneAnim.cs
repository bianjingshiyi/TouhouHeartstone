using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.SelectCardDefineEventArg;

    public class ChooseOneAnim : EventAnimation<Arg>
    {
        public override bool forceQueueAnimations => true;

        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = tEventArg.getVar<Player>(EVENTVAR.PLAYER);
            //自己打出的效果不展示
            if (player == table.player)
            {
                yield break;
            }

            table.setOpponentWaiting(true);

            yield return pauseWhileNotCompleted();

            table.setOpponentWaiting(false);

            yield return stopWhenCanceled();

            var choose = tEventArg.getVar<CardDefine>(EVENTVAR.CHOSEN_CARD);

            var isShow = tEventArg.getVar<bool>(Arg.VAR_SEEN_CARD);
            TableCard chooseCard = null;
            if (isShow)
            {
                var cardPos = table.WorldToTablePosition(table.table.LeftDisplayTransform.position);
                chooseCard = table.createInvokeCard(choose, cardPos, -1, true);
                chooseCard.setFaceup(true);
            }
            if (chooseCard)
            {
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, chooseCard.onShowAnim, chooseCard.animator, _deltaTime))
                    yield return null;
                chooseCard.destroy();
            }
        }
    }
}