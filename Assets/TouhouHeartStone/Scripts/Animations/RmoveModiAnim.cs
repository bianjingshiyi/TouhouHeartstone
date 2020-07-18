using UI;
using UnityEngine;
using TouhouHeartstone;

namespace Game
{
    class RmoveModiAnim : EventAnimation<TouhouCardEngine.Card.RemoveModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, TouhouCardEngine.Card.RemoveModiEventArg eventArg)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    if (atkMod.value > 0)
                        simpleAnim = servant.onAttackDown;
                    else
                        simpleAnim = servant.onAttackUp;
                    servant.AttackTextPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = servant.onLifeDown;
                    else
                        simpleAnim = servant.onLifeUp;
                    servant.HpTextPropNumber.asText.text = eventArg.card.getCurrentLife(table.game).ToString();
                }
            }
            else if (table.tryGetHand(eventArg.card, out var hand))
            {
                animator = hand.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    if (atkMod.value > 0)
                        simpleAnim = hand.onAttackDown;
                    else
                        simpleAnim = hand.onAttackUp;
                    hand.Card.AttackPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = hand.onLifeDown;
                    else
                        simpleAnim = hand.onLifeUp;
                    hand.Card.LifePropNumber.asText.text = eventArg.card.getLife(table.game).ToString();
                }
                else if (eventArg.modifier is CostModifier costMod)
                {
                    if (costMod.value > 0)
                        simpleAnim = hand.onCostDown;
                    else
                        simpleAnim = hand.onCostUp;
                    hand.Card.CostPropNumber.asText.text = eventArg.card.getCost(table.game).ToString();
                }
            }
            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator))
                return false;
            return true;
        }
    }
}
