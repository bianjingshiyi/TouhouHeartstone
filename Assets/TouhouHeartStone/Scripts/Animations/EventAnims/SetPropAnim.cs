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
        public override bool update(TableManager table, Card.SetPropEventArg eventArg, float time)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.propName == PROPERTY.ATTACK && eventArg.value is int attack)
                {
                    servant.attackPropNumber.text.text = attack.ToString();
                    if (eventArg.card.getAttack(table.game) > eventArg.card.define.getAttack())
                    {
                        simpleAnim = servant.attackPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.attackPropNumber.WhiteText;
                    }
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.value is int currentLife)
                {
                    servant.lifePropNumber.text.text = currentLife.ToString();

                    //判断是否是红字
                    if (eventArg.card.getCurrentLife(table.game) < eventArg.card.getLife(table.game))
                    {
                        simpleAnim = servant.lifePropNumber.RedText;
                    }
                    else if (eventArg.card.getLife(table.game) > eventArg.card.define.getLife())
                    {
                        simpleAnim = servant.lifePropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.lifePropNumber.WhiteText;
                    }
                }
                else if (eventArg.propName == PROPERTY.KEYWORDS)
                {
                    if (eventArg.value is string[] keywords)
                    {
                        if (eventArg.beforeValue is string[] beforeKeywords)
                        {

                        }
                        if (keywords.Contains(KEYWORD.FREEZE))
                            servant.freezeImage.display();
                        else
                            servant.freezeImage.hide();
                    }
                    else
                    {
                        servant.freezeImage.hide();
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

                    master.AttackPropNumber.text.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {
                    master.LifePropNumber.text.text = currentLife.ToString();
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

                //判断是否是红字
                if (eventArg.card.getCurrentLife(table.game) < eventArg.card.getLife(table.game))
                {
                    simpleAnim = master.LifePropNumber.RedText;
                }
                else if (eventArg.card.getLife(table.game) > eventArg.card.define.getLife())
                {
                    simpleAnim = master.LifePropNumber.GreenText;
                }
                else
                {
                    simpleAnim = master.LifePropNumber.WhiteText;
                }
                table.setMaster(master, eventArg.card);
            }
            else if (table.tryGetItem(eventArg.card, out var item))
            {
                animator = item.animator;
                if (eventArg.propName == PROPERTY.ATTACK && eventArg.beforeValue is int beforeAttack && eventArg.value is int attack)
                {
                    item.AttackPropNumber.text.text = attack.ToString();
                }
                else if (eventArg.propName == PROPERTY.CURRENT_LIFE && eventArg.beforeValue is int beforeCurrentLife && eventArg.value is int currentLife)
                {

                    item.DurabilityPropNumber.text.text = currentLife.ToString();
                }

                //判断是否是红字
                if (eventArg.card.getCurrentLife(table.game) < eventArg.card.getLife(table.game))
                {
                    simpleAnim = item.DurabilityPropNumber.RedText;
                }
                else if (eventArg.card.getLife(table.game) > eventArg.card.define.getLife())
                {
                    simpleAnim = item.DurabilityPropNumber.GreenText;
                }
                else
                {
                    simpleAnim = item.DurabilityPropNumber.WhiteText;
                }
                table.setItem(item, eventArg.card);
            }


            if (!SimpleAnimHelper.update(table, ref _anim, simpleAnim, animator, time))
                return false;
            return true;
        }
    }
}
