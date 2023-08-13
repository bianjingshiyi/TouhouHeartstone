using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = TouhouCardEngine.Card.PropChangeEventArg;
    class PropChangeAnim : EventAnimation<Arg>
    {
        CardSnapshot _card;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
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
                animator = servant.attackNumber.animator;
                if (propName == DEFINE.ATTACK)
                {
                    if (_card.getAttack(table.game) > _card.define.getAttack())
                    {
                        simpleAnim = servant.attackNumber.textGreenAnim;
                    }
                    else
                    {
                        simpleAnim = servant.attackNumber.textWhiteAnim;
                    }
                }
                else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
                {
                    animator = servant.lifeNumber.animator;

                    //判断是否是红字
                    if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    {
                        simpleAnim = servant.lifeNumber.textRedAnim;
                    }
                    else if (_card.getLife(table.game) > _card.define.getLife())
                    {
                        simpleAnim = servant.lifeNumber.textGreenAnim;
                    }
                    else
                    {
                        simpleAnim = servant.lifeNumber.textWhiteAnim;
                    }
                }
                else if (propName == CARD.SILENCE)
                {
                    simpleAnim = _card.isSilenced(table.game) ? servant.onSilenceTrue : servant.onSilenceFalse;
                }
            }
            else if (table.tryGetMaster(_card, out var master))
            {
                updateMaster(table, master, arg, out animator, out simpleAnim);
            }
            else if (table.tryGetItem(_card, out var item))
            {
                animator = item.durabilityNumber.animator;
                //判断是否是红字
                if (propName == DEFINE.ATTACK && value is int)
                {
                    if (_card.getAttack(table.game) > _card.define.getAttack())
                    {
                        simpleAnim = item.attackNumber.textGreenAnim;
                    }
                    else
                    {
                        simpleAnim = item.attackNumber.textWhiteAnim;
                    }
                }
                else if (propName == CARD.CURRENT_LIFE && value is int)
                {
                    if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    {
                        simpleAnim = item.durabilityNumber.textRedAnim;
                    }
                    else if (_card.getLife(table.game) > _card.define.getLife())
                    {
                        simpleAnim = item.durabilityNumber.textGreenAnim;
                    }
                    else
                    {
                        simpleAnim = item.durabilityNumber.textWhiteAnim;
                    }
                }
            }
            else if (table.tryGetSkill(_card, out var skill))
            {
                var propNum = skill.costNumber;
                animator = propNum.animator;
                if (_card.getCost(table.game) < _card.define.getCost())
                {
                    simpleAnim = propNum.textGreenAnim;
                }
                else if (_card.getCost(table.game) > _card.define.getCost())
                {
                    simpleAnim = propNum.textRedAnim;
                }
                else
                {
                    simpleAnim = propNum.textWhiteAnim;
                }
            }
            else if (table.tryGetHand(_card, out var hand))
            {
                updateHand(table, hand, arg, out animator, out simpleAnim);
            }
            table.updateCardBySnapshot(_card, record);
            if (!animator || !animator.isActiveAndEnabled || simpleAnim == null)
            {
                yield break;
            }
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, simpleAnim, animator, _deltaTime))
            {
                yield return null;
            }
        }

        private void updateMaster(BattleTableManager table, Master master, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;

            animator = null;
            simpleAnim = null;
            switch (propName)
            {
                case DEFINE.LIFE:
                case CARD.CURRENT_LIFE:
                    animator = master.lifeNumber.animator;
                    master.lifeNumber.text = _card.getCurrentLife(table.game).ToString();
                    //判断是否是红字
                    if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    {
                        simpleAnim = master.lifeNumber.textRedAnim;
                    }
                    else if (_card.getLife(table.game) > _card.define.getLife())
                    {
                        simpleAnim = master.lifeNumber.textGreenAnim;
                    }
                    else
                    {
                        simpleAnim = master.lifeNumber.textWhiteAnim;
                    }
                    break;
                case DEFINE.ATTACK:
                    animator = master.attackNumber.animator;
                    if (table.getMasterAttack(_card) > table.getMasterItemAttack(_card) + _card.define.getAttack())
                    {
                        simpleAnim = master.attackNumber.textGreenAnim;
                    }
                    else
                    {
                        simpleAnim = master.attackNumber.textWhiteAnim;
                    }
                    break;
                case CARD.ARMOR:
                    var armor = _card.getArmor(table.game);
                    master.setArmorDisplay(armor > 0);
                    master.setArmor(armor);
                    break;
            }
        }

        private void updateHand(BattleTableManager table, HandItem hand, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;
            var cardType = _card.define.type;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.ATTACK)
            {
                var attackNumber = hand.card.getAttackPropNum(cardType);
                if (!attackNumber)
                    return;

                animator = attackNumber.animator;
                if (_card.getAttack(table.game) > _card.define.getAttack())
                    simpleAnim = attackNumber.textGreenAnim;
                else
                    simpleAnim = attackNumber.textWhiteAnim;
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                var lifeNumber = hand.card.getLifePropNum(cardType);
                if (!lifeNumber)
                    return;

                animator = lifeNumber.animator;
                if (_card.getCurrentLife(table.game) < _card.getLife(table.game))
                    simpleAnim = lifeNumber.textRedAnim;
                else if (_card.getLife(table.game) > _card.define.getLife())
                    simpleAnim = lifeNumber.textGreenAnim;
                else
                    simpleAnim = lifeNumber.textWhiteAnim;
            }
            else if (propName == DEFINE.COST)
            {
                var costNumber = hand.card.getCostPropNum(cardType);
                if (!costNumber)
                    return;

                animator = costNumber.animator;
                if (_card.getCost(table.game) < _card.define.getCost())
                    simpleAnim = costNumber.textGreenAnim;
                else if (_card.getCost(table.game) > _card.define.getCost())
                    simpleAnim = costNumber.textRedAnim;
                else
                    simpleAnim = costNumber.textWhiteAnim;
            }
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            return false;
        }
    }
}
