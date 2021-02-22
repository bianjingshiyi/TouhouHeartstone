using UI;
using UnityEngine;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;
namespace Game
{
    class AddModiAnim : EventAnimation<Card.AddModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, Card.AddModiEventArg eventArg, float time)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {

                    servant.attackPropNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = servant.attackPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.attackPropNumber.WhiteText;
                    }
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    //if (lifMod.value > 0)
                    //    simpleAnim = servant.onLifeUp;
                    //else
                    //    simpleAnim = servant.onLifeDown;
                    servant.lifePropNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
            }
            else if (table.tryGetHand(eventArg.card, out var hand))
            {
                animator = hand.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    PropNumber propNumber = hand.Card.getAttackPropNum(eventArg.card);
                    propNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = propNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = propNumber.WhiteText;
                    }
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    PropNumber propNumber = hand.Card.getLifePropNum(eventArg.card);
                    propNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                    if (eventArg.card.getLife(table.game) > eventArg.card.define.getLife())
                    {
                        simpleAnim = propNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = propNumber.WhiteText;
                    }
                }
                else if (eventArg.modifier is CostModifier costMod)
                {
                    PropNumber propNumber = hand.Card.getCostPropNum(eventArg.card);
                    //propNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                    propNumber.text.text = eventArg.card.getCost(table.game).ToString();
                    animator = propNumber.asAnimator;
                    if (eventArg.card.getCost(table.game) < eventArg.card.define.getCost())
                    {
                        animator.SetBool("isGreen", true);
                        animator.SetBool("isRed", false);
                        //simpleAnim = propNumber.GreenText;
                    }
                    else if (eventArg.card.getCost(table.game) > eventArg.card.define.getCost())
                    {
                        animator.SetBool("isRed", true);
                        animator.SetBool("isGreen", false);
                        //simpleAnim = propNumber.RedText;
                    }
                    else
                    {
                        animator.SetBool("isGreen", false);
                        animator.SetBool("isRed", false);
                        //simpleAnim = propNumber.WhiteText;
                    }
                }
            }
            else if (table.tryGetMaster(eventArg.card, out var master))
            {
                animator = master.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    master.AttackPropNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
                table.setMaster(master, eventArg.card);
            }
            else if (table.tryGetItem(eventArg.card, out var item))
            {
                animator = item.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    item.AttackPropNumber.text.text = eventArg.valueAfter is int i ? i.ToString() : "?";
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
