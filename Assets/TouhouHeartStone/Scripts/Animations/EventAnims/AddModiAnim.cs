using UI;
using UnityEngine;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;
namespace Game
{
    class AddModiAnim : EventAnimation<Card.AddModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, Card.AddModiEventArg eventArg)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    if (atkMod.value > 0)
                        simpleAnim = servant.onAttackUp;
                    else
                        simpleAnim = servant.onAttackDown;
                    servant.AttackTextPropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = servant.onLifeUp;
                    else
                        simpleAnim = servant.onLifeDown;
                    servant.HpTextPropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
            }
            else if (table.tryGetHand(eventArg.card, out var hand))
            {
                animator = hand.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    if (atkMod.value > 0)
                        simpleAnim = hand.onAttackUp;
                    else
                        simpleAnim = hand.onAttackDown;
                    hand.Card.AttackPropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = hand.onLifeUp;
                    else
                        simpleAnim = hand.onLifeDown;
                    hand.Card.LifePropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
                else if (eventArg.modifier is CostModifier costMod)
                {
                    if (costMod.value > 0)
                        simpleAnim = hand.onCostUp;
                    else
                        simpleAnim = hand.onCostDown;
                    hand.Card.CostPropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
            }
            else if (table.tryGetMaster(eventArg.card, out var master))
            {
                animator = master.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    //if (atkMod.value > 0)
                    //    simpleAnim = master.onAttackUp;
                    //else
                    //    simpleAnim = master.onAttackDown;
                    master.AttackPropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
                table.setMaster(master, eventArg.card);
            }
            else if (table.tryGetItem(eventArg.card, out var item))
            {
                animator = item.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    //if (atkMod.value > 0)
                    //    simpleAnim = master.onAttackUp;
                    //else
                    //    simpleAnim = master.onAttackDown;
                    item.AttackPropNumber.asText.text = eventArg.valueAfter is int i ? i.ToString() : "?";
                }
            }
            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator))
                return false;
            return true;
        }
    }
}
