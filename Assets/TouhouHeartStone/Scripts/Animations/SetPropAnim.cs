using UI;
using UnityEngine;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;
using System.Linq;
using BJSYGameCore.UI;

namespace Game
{
    class SetPropAnim : EventAnimation<Card.SetPropEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, Card.SetPropEventArg eventArg)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.propName == PROPERTY.ATTACK && eventArg.beforeValue is int beforeAttack && eventArg.value is int attack)
                {
                    if (attack > beforeAttack)
                        simpleAnim = servant.onAttackUp;
                    else
                        simpleAnim = servant.onAttackDown;
                    servant.AttackTextPropNumber.asText.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
                    if (currentLife > beforeCurrentLife)
                        simpleAnim = servant.onLifeUp;
                    else
                        simpleAnim = servant.onLifeDown;
                    servant.HpTextPropNumber.asText.text = currentLife.ToString();
                }
                else if (eventArg.propName == PROPERTY.KEYWORDS)
                {
                    if (eventArg.value is string[] keywords)
                    {
                        if (eventArg.beforeValue is string[] beforeKeywords)
                        {

                        }
                        if (keywords.Contains(KEYWORD.FREEZE))
                            servant.FreezeImage.display();
                        else
                            servant.FreezeImage.hide();
                    }
                    else
                    {
                        servant.FreezeImage.hide();
                    }
                }
                else if (eventArg.propName == PROPERTY.SILENCE && eventArg.beforeValue is bool beforeSilence && eventArg.value is bool silence)
                {

                }
                table.setServant(servant, eventArg.card);
            }
            else if (table.tryGetMaster(eventArg.card, out var master))
            {
                animator = master.animator;
                if (eventArg.propName == PROPERTY.ATTACK && eventArg.beforeValue is int beforeAttack && eventArg.value is int attack)
                {
                    //if (attack > beforeAttack)
                    //    simpleAnim = master.onAttackUp;
                    //else
                    //    simpleAnim = master.onAttackDown;
                    master.AttackPropNumber.asText.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
                    //if (currentLife > beforeCurrentLife)
                    //    simpleAnim = master.onLifeUp;
                    //else
                    //    simpleAnim = master.onLifeDown;
                    master.LifePropNumber.asText.text = currentLife.ToString();
                }
                else if (eventArg.propName == PROPERTY.KEYWORDS)
                {
                    if (eventArg.value is string[] keywords)
                    {
                        if (eventArg.beforeValue is string[] beforeKeywords)
                        {

                        }
                        if (keywords.Contains(KEYWORD.FREEZE))
                            master.FreezeImage.display();
                        else
                            master.FreezeImage.hide();
                    }
                    else
                    {
                        master.FreezeImage.hide();
                    }
                }
                else if (eventArg.propName == PROPERTY.SILENCE && eventArg.beforeValue is bool beforeSilence && eventArg.value is bool silence)
                {

                }
                table.setMaster(master, eventArg.card);
            }
            else if (table.tryGetItem(eventArg.card, out var item))
            {
                animator = item.animator;
                if (eventArg.propName == PROPERTY.ATTACK && eventArg.beforeValue is int beforeAttack && eventArg.value is int attack)
                {
                    //if (attack > beforeAttack)
                    //    simpleAnim = item.onAttackUp;
                    //else
                    //    simpleAnim = item.onAttackDown;
                    item.AttackPropNumber.asText.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
                    //if (currentLife > beforeCurrentLife)
                    //    simpleAnim = item.onLifeUp;
                    //else
                    simpleAnim = item.onLifeDown;
                    item.DurabilityPropNumber.asText.text = currentLife.ToString();
                }
            }
            //else
            //    throw new ActorNotFoundException(eventArg.card);
            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator))
                return false;
            return true;
        }
    }
}
