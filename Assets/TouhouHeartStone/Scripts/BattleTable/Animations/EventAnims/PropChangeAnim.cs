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
            var value = arg.value;

            _card.setProp(propName, value);
            getSimpleAnimAndAnimator(table, arg, out var animator, out var simpleAnim);
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

        private void getSimpleAnimAndAnimator(BattleTableManager table, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            simpleAnim = null;
            animator = null;
            if (table.tryGetServant(_card, out var servant))
            {
                updateServant(table, servant, arg, out animator, out simpleAnim);
            }
            else if (table.tryGetMaster(_card, out var master))
            {
                updateMaster(table, master, arg, out animator, out simpleAnim);
            }
            else if (table.tryGetItem(_card, out var item))
            {
                updateItem(table, item, arg, out animator, out simpleAnim);
            }
            else if (table.tryGetSkill(_card, out var skill))
            {
                updateSkill(table, skill, arg, out animator, out simpleAnim);
            }
            else if (table.tryGetHand(_card, out var hand))
            {
                updateHand(table, hand, arg, out animator, out simpleAnim);
            }
        }
        private void updateServant(BattleTableManager table, Servant servant, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.ATTACK)
            {
                int attack = value is int intValue ? intValue : _card.getAttack(table.game);
                animator = servant.attackNumber.animator;
                simpleAnim = attack > _card.define.getAttack() ? servant.attackNumber.textGreenAnim : servant.attackNumber.textWhiteAnim;
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : _card.getLife(table.game);
                int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : _card.getCurrentLife(table.game);
                animator = servant.lifeNumber.animator;
                //判断是否是红字
                if (curLife < life)
                    simpleAnim = servant.lifeNumber.textRedAnim;
                else if (life > _card.define.getLife())
                    simpleAnim = servant.lifeNumber.textGreenAnim;
                else
                    simpleAnim = servant.lifeNumber.textWhiteAnim;
            }
            else if (propName == CARD.SILENCE)
            {
                bool silenced = value is bool boolValue ? boolValue : _card.isSilenced(table.game);
                simpleAnim = silenced ? servant.onSilenceTrue : servant.onSilenceFalse;
            }
        }
        private void updateMaster(BattleTableManager table, Master master, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            switch (propName)
            {
                case DEFINE.LIFE:
                case CARD.CURRENT_LIFE:
                    {
                        int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : _card.getLife(table.game);
                        int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : _card.getCurrentLife(table.game);
                        animator = master.lifeNumber.animator;
                        //判断是否是红字
                        if (curLife < life)
                            simpleAnim = master.lifeNumber.textRedAnim;
                        else if (life > _card.define.getLife())
                            simpleAnim = master.lifeNumber.textGreenAnim;
                        else
                            simpleAnim = master.lifeNumber.textWhiteAnim;
                        break;
                    }
                case DEFINE.ATTACK:
                    {
                        int attack = value is int intValue ? intValue : _card.getAttack(table.game);
                        animator = master.attackNumber.animator;
                        simpleAnim = attack > _card.define.getAttack() ? master.attackNumber.textGreenAnim : master.attackNumber.textWhiteAnim;
                        break;
                    }
            }
        }
        private void updateItem(BattleTableManager table, Item item, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            //判断是否是红字
            if (propName == DEFINE.ATTACK)
            {
                int attack = value is int intValue ? intValue : _card.getAttack(table.game);
                animator = item.attackNumber.animator;
                simpleAnim = attack > _card.define.getAttack() ? item.attackNumber.textGreenAnim : item.attackNumber.textWhiteAnim;
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : _card.getLife(table.game);
                int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : _card.getCurrentLife(table.game);
                animator = item.durabilityNumber.animator;
                //判断是否是红字
                if (curLife < life)
                    simpleAnim = item.durabilityNumber.textRedAnim;
                else if (life > _card.define.getLife())
                    simpleAnim = item.durabilityNumber.textGreenAnim;
                else
                    simpleAnim = item.durabilityNumber.textWhiteAnim;
            }
        }
        private void updateSkill(BattleTableManager table, Skill skill, Arg arg, out Animator animator, out SimpleAnim simpleAnim)
        {
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.COST)
            {
                var propNum = skill.costNumber;
                int cost = value is int intValue ? intValue : _card.getCost(table.game);
                int defineCost = _card.define.getCost();
                animator = propNum.animator;
                if (cost < defineCost)
                    simpleAnim = propNum.textGreenAnim;
                else if (cost > defineCost)
                    simpleAnim = propNum.textRedAnim;
                else
                    simpleAnim = propNum.textWhiteAnim;
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
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}
