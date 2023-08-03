using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = Card.SetDefineEventArg;
    class SetDefineAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var card = table.getRecordCardSnapshot(record, Arg.VAR_CARD);
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(card, out var servant))
            {
                simpleAnim = servant.onSetDefine;
                animator = servant.animator;
                table.setServant(servant, card);
            }
            else if (table.tryGetMaster(card, out var master))
            {
                simpleAnim = master.onSetDefine;
                animator = master.animator;
                table.setMaster(master, card);
            }
            else if (table.tryGetItem(card, out var item))
            {
                simpleAnim = item.onSetDefine;
                animator = item.animator;
                table.setItem(item, card);
            }
            else if (table.tryGetSkill(card, out var skill))
            {
                simpleAnim = skill.onSetDefine;
                animator = skill.animator;
                table.setSkill(skill, card);
            }
            else if (table.tryGetHand(card, out var hand))
            {
                simpleAnim = hand.onSetDefine;
                animator = hand.animator;
                if (card.getOwner() == table.player)
                    table.setCard(hand.card, card, true);
            }
            if (simpleAnim != null && animator)
            {
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, simpleAnim, animator, _time))
                    yield return null;
            }
        }
    }
}
