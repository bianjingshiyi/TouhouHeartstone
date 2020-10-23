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
                    
                    servant.AttackTextPropNumber.asText.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
                    
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

                if (table.tryGetAllPropNumber(eventArg.card, out var numbers))
                {
                    animator = numbers[0].animator;
                    //生命值
                    if (eventArg.card.getCurrentLife(table.game) < eventArg.card.getLife(table.game))
                    {
                        simpleAnim = numbers[0].RedText;
                    }
                    else if (eventArg.card.getLife(table.game) > eventArg.card.define.getLife())
                    {
                        simpleAnim = numbers[0].GreenText;
                    }
                    else
                    {
                        simpleAnim = numbers[0].WhiteText;
                    }

                    //攻击力
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = numbers[1].GreenText;
                    }
                    else
                    {
                        simpleAnim = numbers[1].WhiteText;
                    }

                }

            }
            else if (table.tryGetMaster(eventArg.card, out var master))
            {
                animator = master.animator;
                if (eventArg.propName == PROPERTY.ATTACK && eventArg.beforeValue is int beforeAttack && eventArg.value is int attack)
                {
                    
                    master.AttackPropNumber.asText.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
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
                    item.AttackPropNumber.asText.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
                   
                    item.DurabilityPropNumber.asText.text = currentLife.ToString();
                }
            }

            
            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator))
                return false;
            return true;
        }
    }
}
