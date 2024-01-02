using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = SelectCardDefineEventDefine;

    public class ChooseOneAnim : EventAnimation<Arg>
    {
        public override bool forceQueueAnimations => true;

        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            //自己打出的效果不展示
            if (player == table.player)
            {
                yield break;
            }

            table.setOpponentWaiting(true);

            yield return pauseWhileNotCompleted();

            table.setOpponentWaiting(false);

            stopWhenCanceled(table);

            var choose = eventArg.getVar<CardDefine>(EVENTVAR.CHOSEN_CARD);

            var isShow = eventArg.getVar<bool>(Arg.VAR_CARD_SEEN);
            TableCard chooseCard = null;
            if (isShow)
            {
                var cardPos = table.WorldToTablePosition(table.table.LeftDisplayTransform.position);
                chooseCard = table.createInvokeCard(choose, cardPos, -1);
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