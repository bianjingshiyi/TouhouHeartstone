using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = RevealCardDefineEventDefine;
    public class RevealCardDefineAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var invisibleForSelf = eventArg.getVar<bool>(Arg.VAR_INVISIBLE_FOR_SELF);
            var invisibleForOppo = eventArg.getVar<bool>(Arg.VAR_INVISIBLE_FOR_OPPO);
            bool isSelf = player == table.player;
            if (isSelf && invisibleForSelf)
                yield break;
            if (!isSelf && invisibleForOppo)
                yield break;
            var define = eventArg.getVar<CardDefine>(EVENTVAR.CARD_DEFINE);

            var cardPos = table.WorldToTablePosition(table.table.LeftDisplayTransform.position);
            TableCard uiCard = table.createInvokeCard(define, cardPos, -1);
            if (uiCard)
            {
                uiCard.setFaceup(true);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, uiCard.onShowAnim, uiCard.animator, _deltaTime))
                    yield return null;
                uiCard.destroy();
            }
            yield break;
        }
    }
}
