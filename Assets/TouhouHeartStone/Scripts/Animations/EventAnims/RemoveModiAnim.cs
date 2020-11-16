using UI;
using UnityEngine;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;

namespace Game
{
    class RemoveModiAnim : EventAnimation<Card.RemoveModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, Card.RemoveModiEventArg eventArg, float time)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    servant.AttackTextPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = servant.AttackTextPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.AttackTextPropNumber.WhiteText;
                    }
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    servant.HpTextPropNumber.asText.text = eventArg.card.getCurrentLife(table.game).ToString();
                }
            }
            else if (table.tryGetHand(eventArg.card, out var hand))
            {
                animator = hand.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    hand.Card.AttackPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = hand.Card.AttackPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = hand.Card.AttackPropNumber.WhiteText;
                    }
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    hand.Card.LifePropNumber.asText.text = eventArg.card.getLife(table.game).ToString();
                    if (eventArg.card.getLife(table.game) > eventArg.card.define.getLife())
                    {
                        simpleAnim = hand.Card.LifePropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = hand.Card.LifePropNumber.WhiteText;
                    }
                }
                else if (eventArg.modifier is CostModifier costMod)
                {
                    hand.Card.CostPropNumber.asText.text = eventArg.card.getCost(table.game).ToString();
                    if (eventArg.card.getCost(table.game) < eventArg.card.define.getCost())
                    {
                        simpleAnim = hand.Card.CostPropNumber.GreenText;
                    }
                    else if (eventArg.card.getCost(table.game) > eventArg.card.define.getCost())
                    {
                        simpleAnim = hand.Card.CostPropNumber.RedText;
                    }
                    else
                    {
                        simpleAnim = hand.Card.CostPropNumber.WhiteText;
                    }
                }
            }
            else if (table.tryGetMaster(eventArg.card, out var master))
            {
                animator = master.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    master.AttackPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                }
                table.setMaster(master, eventArg.card);
            }
            else if (table.tryGetItem(eventArg.card, out var item))
            {
                animator = item.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    item.AttackPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = item.AttackPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = item.AttackPropNumber.WhiteText;
                    }
                }
            }
            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator, time))
                return false;
            return true;
        }
    }
}
