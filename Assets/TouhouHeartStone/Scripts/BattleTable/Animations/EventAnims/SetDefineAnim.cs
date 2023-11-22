using System.Collections;
using TouhouCardEngine;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = Card.SetDefineEventArg;
    class SetDefineAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            var card = table.getRecordCardSnapshot(record, Arg.VAR_CARD);
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(card, out var servant))
            {
                simpleAnim = servant.onSetDefine;
                animator = servant.animator;
            }
            else if (table.tryGetMaster(card, out var master))
            {
                simpleAnim = master.onSetDefine;
                animator = master.animator;
            }
            else if (table.tryGetItem(card, out var item))
            {
                simpleAnim = item.onSetDefine;
                animator = item.animator;
            }
            else if (table.tryGetSecret(card, out var secret))
            {
                simpleAnim = secret.onSetDefine;
                animator = secret.animator;
            }
            else if (table.tryGetSkill(card, out var skill))
            {
                simpleAnim = skill.onSetDefine;
                animator = skill.animator;
            }
            else if (table.tryGetHand(card, out var hand))
            {
                simpleAnim = hand.onSetDefine;
                animator = hand.animator;
            }
            table.updateCardBySnapshot(card, record);
            if (simpleAnim != null && animator)
            {
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, simpleAnim, animator, _deltaTime))
                    yield return null;
            }
        }
    }
}
