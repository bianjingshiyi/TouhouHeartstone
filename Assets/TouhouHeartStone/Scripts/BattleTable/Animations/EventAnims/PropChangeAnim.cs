using System.Collections;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = TouhouCardEngine.Card.PropChangeEventArg;
    class PropChangeAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(record, Arg.VAR_CARD);
            var propName = record.getVar<string>(Arg.VAR_PROP_NAME);
            var value = record.getVar(Arg.VAR_VALUE_AFTER);

            card.setProp(propName, value);
            getSimpleAnimAndAnimator(table, card, out var animator, out var simpleAnim);
            table.updateCardBySnapshot(card);
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

        private void getSimpleAnimAndAnimator(BattleTableManager table, ICardData card, out Animator animator, out SimpleAnim simpleAnim)
        {
            simpleAnim = null;
            animator = null;
            if (table.tryGetServant(card, out var servant))
            {
                updateServant(table, card, servant, out animator, out simpleAnim);
            }
            else if (table.tryGetMaster(card, out var master))
            {
                updateMaster(table, card, master, out animator, out simpleAnim);
            }
            else if (table.tryGetItem(card, out var item))
            {
                updateItem(table, card, item, out animator, out simpleAnim);
            }
            else if (table.tryGetSkill(card, out var skill))
            {
                updateSkill(table, card, skill, out animator, out simpleAnim);
            }
            else if (table.tryGetHand(card, out var hand))
            {
                updateHand(table, card, hand, out animator, out simpleAnim);
            }
        }
        private void updateServant(BattleTableManager table, ICardData card, Servant servant, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = tEventArg;
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.ATTACK)
            {
                int attack = value is int intValue ? intValue : card.getAttack(table.game);
                animator = servant.attackNumber.animator;
                simpleAnim = attack > card.define.getAttack() ? servant.attackNumber.textGreenAnim : servant.attackNumber.textWhiteAnim;
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : card.getLife(table.game);
                int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : card.getCurrentLife(table.game);
                animator = servant.lifeNumber.animator;
                //判断是否是红字
                if (curLife < life)
                    simpleAnim = servant.lifeNumber.textRedAnim;
                else if (life > card.define.getLife())
                    simpleAnim = servant.lifeNumber.textGreenAnim;
                else
                    simpleAnim = servant.lifeNumber.textWhiteAnim;
            }
            else if (propName == CARD.SILENCE)
            {
                bool silenced = value is bool boolValue ? boolValue : card.isSilenced(table.game);
                simpleAnim = silenced ? servant.onSilenceTrue : servant.onSilenceFalse;
            }
        }
        private void updateMaster(BattleTableManager table, ICardData card, Master master, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = tEventArg;
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            switch (propName)
            {
                case DEFINE.LIFE:
                case CARD.CURRENT_LIFE:
                    {
                        int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : card.getLife(table.game);
                        int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : card.getCurrentLife(table.game);
                        animator = master.lifeNumber.animator;
                        //判断是否是红字
                        if (curLife < life)
                            simpleAnim = master.lifeNumber.textRedAnim;
                        else if (life > card.define.getLife())
                            simpleAnim = master.lifeNumber.textGreenAnim;
                        else
                            simpleAnim = master.lifeNumber.textWhiteAnim;
                        break;
                    }
                case DEFINE.ATTACK:
                    {
                        int attack = value is int intValue ? intValue : card.getAttack(table.game);
                        animator = master.attackNumber.animator;
                        simpleAnim = attack > table.getMasterItemAttack(card) + card.define.getAttack() ? master.attackNumber.textGreenAnim : master.attackNumber.textWhiteAnim;
                        break;
                    }
            }
        }
        private void updateItem(BattleTableManager table, ICardData card, Item item, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = tEventArg;
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            //判断是否是红字
            if (propName == DEFINE.ATTACK)
            {
                int attack = value is int intValue ? intValue : card.getAttack(table.game);
                animator = item.attackNumber.animator;
                simpleAnim = attack > card.define.getAttack() ? item.attackNumber.textGreenAnim : item.attackNumber.textWhiteAnim;
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : card.getLife(table.game);
                int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : card.getCurrentLife(table.game);
                animator = item.durabilityNumber.animator;
                //判断是否是红字
                if (curLife < life)
                    simpleAnim = item.durabilityNumber.textRedAnim;
                else if (life > card.define.getLife())
                    simpleAnim = item.durabilityNumber.textGreenAnim;
                else
                    simpleAnim = item.durabilityNumber.textWhiteAnim;
            }
        }
        private void updateSkill(BattleTableManager table, ICardData card, Skill skill, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = tEventArg;
            var propName = arg.propName;
            var value = arg.value;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.COST)
            {
                var propNum = skill.costNumber;
                int cost = value is int intValue ? intValue : card.getCost(table.game);
                int defineCost = card.define.getCost();
                animator = propNum.animator;
                if (cost < defineCost)
                    simpleAnim = propNum.textGreenAnim;
                else if (cost > defineCost)
                    simpleAnim = propNum.textRedAnim;
                else
                    simpleAnim = propNum.textWhiteAnim;
            }
        }
        private void updateHand(BattleTableManager table, ICardData card, HandItem hand, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = tEventArg;
            var propName = arg.propName;
            var cardType = card.define.type;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.ATTACK)
            {
                var attackNumber = hand.card.getAttackPropNum(cardType);
                if (!attackNumber)
                    return;

                animator = attackNumber.animator;
                if (card.getAttack(table.game) > card.define.getAttack())
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
                if (card.getCurrentLife(table.game) < card.getLife(table.game))
                    simpleAnim = lifeNumber.textRedAnim;
                else if (card.getLife(table.game) > card.define.getLife())
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
                if (card.getCost(table.game) < card.define.getCost())
                    simpleAnim = costNumber.textGreenAnim;
                else if (card.getCost(table.game) > card.define.getCost())
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
