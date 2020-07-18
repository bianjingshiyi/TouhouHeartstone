using UI;
using UnityEngine;
using TouhouHeartstone;

namespace Game
{
    class AddModiAnim : EventAnimation<TouhouCardEngine.Card.AddModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, TouhouCardEngine.Card.AddModiEventArg eventArg)
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
                    servant.AttackTextPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = servant.onLifeUp;
                    else
                        simpleAnim = servant.onLifeDown;
                    servant.HpTextPropNumber.asText.text = eventArg.card.getCurrentLife(table.game).ToString();
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
                    hand.Card.AttackPropNumber.asText.text = eventArg.card.getAttack(table.game).ToString();
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = hand.onLifeUp;
                    else
                        simpleAnim = hand.onLifeDown;
                    hand.Card.LifePropNumber.asText.text = eventArg.card.getLife(table.game).ToString();
                }
                else if (eventArg.modifier is CostModifier costMod)
                {
                    if (costMod.value > 0)
                        simpleAnim = hand.onCostUp;
                    else
                        simpleAnim = hand.onCostDown;
                    hand.Card.CostPropNumber.asText.text = eventArg.card.getCost(table.game).ToString();
                }
            }
            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator))
                return false;
            return true;
        }
    }
}
