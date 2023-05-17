using UI;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;
using TouhouCardEngine;
using System.Collections;
using UnityEngine;

namespace Game
{
    using Arg = Card.SetDefineEventArg;
    class SetDefineAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var card = table.getRecordCardSnapshot(record, Arg.VAR_CARD);
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(card, out var servant))
            {
                simpleAnim = servant.onSetDefine;
                animator = servant.servantAnimator;
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
                    table.setCard(hand.Card, card, true);
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
