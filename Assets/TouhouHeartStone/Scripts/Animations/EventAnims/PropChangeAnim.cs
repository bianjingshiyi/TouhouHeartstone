using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    using Arg = TouhouCardEngine.Card.PropChangeEventArg;
    class PropChangeAnim : EventAnimation<Arg>
    {
        CardSnapshot _card;
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            _card = table.getRecordCardSnapshot(record, Arg.VAR_CARD);
            var propName = arg.propName;
            var beforeValue = arg.beforeValue;
            var value = arg.value;

            SimpleAnim simpleAnim = null;
            Animator animator = null;
            _card.setProp(propName, value);
            if (table.tryGetServant(_card, out var servant))
            {
                animator = servant.attackPropNumber.asAnimator;
                if (propName == DEFINE.ATTACK)
                {
                    if (_card.getAttack(table.game) > _card.define.getAttack())
                    {
                        simpleAnim = servant.attackPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.attackPropNumber.WhiteText;
                    }
                }
                else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
                {
                    animator = servant.lifePropNumber.asAnimator;

                    //判断是否是红字
                    if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    {
                        simpleAnim = servant.lifePropNumber.RedText;
                    }
                    else if (_card.getLife(table.game) > _card.define.getLife())
                    {
                        simpleAnim = servant.lifePropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.lifePropNumber.WhiteText;
                    }
                }
                else if (propName == CARD.SILENCE)
                {
                    simpleAnim = _card.isSilenced(table.game) ? servant.onSilenceTrue : servant.onSilenceFalse;
                }

                table.setServant(servant, _card);
            }
            else if (table.tryGetMaster(_card, out var master))
            {
                updateMaster(table, record, master, arg, out animator, out simpleAnim);
            }
            else if (table.tryGetItem(_card, out var item))
            {
                animator = item.DurabilityPropNumber.animator;
                //判断是否是红字
                if (propName == DEFINE.ATTACK && value is int)
                {
                    if (_card.getAttack(table.game) > _card.define.getAttack())
                    {
                        simpleAnim = servant.attackPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = servant.attackPropNumber.WhiteText;
                    }
                }
                else if (propName == CARD.CURRENT_LIFE && value is int)
                {
                    if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    {
                        simpleAnim = item.DurabilityPropNumber.RedText;
                    }
                    else if (_card.getLife(table.game) > _card.define.getLife())
                    {
                        simpleAnim = item.DurabilityPropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = item.DurabilityPropNumber.WhiteText;
                    }
                }
                table.setItem(item, _card);
            }
            else if (table.tryGetSkill(_card, out var skill))
            {
                var propNum = skill.costPropNumber;
                animator = propNum.asAnimator;
                if (_card.getCost(table.game) < _card.define.getCost())
                {
                    propNum.GreenText.invokeBeforeAnim();
                }
                else if (_card.getCost(table.game) > _card.define.getCost())
                {
                    propNum.RedText.invokeBeforeAnim();
                }
                else
                {
                    propNum.WhiteText.invokeBeforeAnim();
                }
                table.setSkill(skill, _card);
            }
            else if (table.tryGetHand(_card, out var hand))
            {
                var cardType = _card.define.type;
                var propNum = hand.Card.getCostPropNum(cardType);
                animator = hand.Card.getCostPropNum(cardType).asAnimator;
                if (_card.getCost(table.game) < _card.define.getCost())
                {
                    propNum.GreenText.invokeBeforeAnim();
                }
                else if (_card.getCost(table.game) > _card.define.getCost())
                {
                    propNum.RedText.invokeBeforeAnim();
                }
                else
                {
                    propNum.WhiteText.invokeBeforeAnim();
                }
                table.setCard(hand.Card, _card, _card.getOwner() == table.player);
            }

            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, simpleAnim, animator, _time))
            {
                yield return null;
            }
        }

        private void updateMaster(TableManager table, EventRecord record, Master master, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;

            animator = null;
            simpleAnim = null;
            switch (propName)
            {
                case DEFINE.LIFE:
                case CARD.CURRENT_LIFE:
                    animator = master.LifePropNumber.animator;
                    master.LifePropNumber.text.text = _card.getCurrentLife(table.game).ToString();
                    //判断是否是红字
                    if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    {
                        simpleAnim = master.LifePropNumber.RedText;
                    }
                    else if (_card.getLife(table.game) > _card.define.getLife())
                    {
                        simpleAnim = master.LifePropNumber.GreenText;
                    }
                    else
                    {
                        simpleAnim = master.LifePropNumber.WhiteText;
                    }
                    break;
                case CARD.ARMOR:
                    var armor = _card.getArmor(table.game);
                    if (armor > 0)
                    {
                        master.setBoolTrue("hasArmor");
                        master.ArmorPropNumber.text.text = armor.ToString();
                    }
                    else
                        master.setBoolFalse("hasArmor");
                    break;
            }
            table.setMaster(master, _card);
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            return false;
        }
    }
}
