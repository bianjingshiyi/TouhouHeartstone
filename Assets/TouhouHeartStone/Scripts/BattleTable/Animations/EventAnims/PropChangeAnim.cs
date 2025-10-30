using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = TouhouCardEngine.CardPropChangeEventDefine;
    class PropChangeAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, Arg.VAR_CARD);
            var propName = eventArg.getVar<string>(Arg.VAR_PROP_NAME);
            var value = eventArg.getVar(Arg.VAR_VALUE_AFTER);

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
            var arg = eventArg;
            var propName = arg.getVar<string>(Arg.VAR_PROP_NAME);
            var value = arg.getVar<object>(Arg.VAR_VALUE_AFTER);
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.ATTACK)
            {
                servant.attackNumber.setTextColor(getAttackColor(table, card));
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                servant.lifeNumber.setTextColor(getLifeColor(table, card));
            }
            else if (propName == CARD.SILENCE)
            {
                bool silenced = value is bool boolValue ? boolValue : card.isSilenced(table.game);
                simpleAnim = silenced ? servant.onSilenceTrue : servant.onSilenceFalse;
            }
        }
        private void updateMaster(BattleTableManager table, ICardData card, Master master, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = eventArg;
            var propName = arg.getVar<string>(Arg.VAR_PROP_NAME);
            animator = null;
            simpleAnim = null;
            switch (propName)
            {
                case DEFINE.LIFE:
                case CARD.CURRENT_LIFE:
                    master.lifeNumber.setTextColor(getLifeColor(table, card));
                    break;
                case DEFINE.ATTACK:
                    master.attackNumber.setTextColor(getMasterAttackColor(table, card));
                    break;
            }
        }
        private void updateItem(BattleTableManager table, ICardData card, Item item, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = eventArg;
            var propName = arg.getVar<string>(Arg.VAR_PROP_NAME);
            animator = null;
            simpleAnim = null;
            //判断是否是红字
            if (propName == DEFINE.ATTACK)
            {
                item.attackNumber.setTextColor(getAttackColor(table, card));
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                item.durabilityNumber.setTextColor(getLifeColor(table, card));
            }
        }
        private void updateSkill(BattleTableManager table, ICardData card, Skill skill, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = eventArg;
            var propName = arg.getVar<string>(Arg.VAR_PROP_NAME);
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.COST)
            {
                skill.costNumber.setTextColor(getCostColor(table, card));
            }
        }
        private void updateHand(BattleTableManager table, ICardData card, HandItem hand, out Animator animator, out SimpleAnim simpleAnim)
        {
            var arg = eventArg;
            var propName = arg.getVar<string>(Arg.VAR_PROP_NAME);
            var cardType = card.define.type;
            animator = null;
            simpleAnim = null;
            if (propName == DEFINE.ATTACK)
            {
                var attackNumber = hand.card.getAttackPropNum(cardType);
                if (!attackNumber)
                    return;

                attackNumber.setTextColor(getAttackColor(table, card));
            }
            else if (propName == CARD.CURRENT_LIFE || propName == DEFINE.LIFE)
            {
                var lifeNumber = hand.card.getLifePropNum(cardType);
                if (!lifeNumber)
                    return;

                lifeNumber.setTextColor(getLifeColor(table, card));
            }
            else if (propName == DEFINE.COST)
            {
                var costNumber = hand.card.getCostPropNum(cardType);
                if (!costNumber)
                    return;

                costNumber.setTextColor(getCostColor(table, card));
            }
        }
        private PropertyNumberColor getAttackColor(BattleTableManager table, ICardData card)
        {
            var arg = eventArg;
            var value = arg.getVar<object>(Arg.VAR_VALUE_AFTER);
            int attack = value is int intValue ? intValue : card.getAttack(table.game);
            return attack > card.define.getAttack() ? PropertyNumberColor.Green : PropertyNumberColor.White;
        }
        private PropertyNumberColor getMasterAttackColor(BattleTableManager table, ICardData card)
        {
            var arg = eventArg;
            var value = arg.getVar<object>(Arg.VAR_VALUE_AFTER);
            int attack = value is int intValue ? intValue : card.getAttack(table.game);
            return attack > table.getMasterItemAttack(card) + card.define.getAttack() ? PropertyNumberColor.Green : PropertyNumberColor.White;
        }
        private PropertyNumberColor getLifeColor(BattleTableManager table, ICardData card)
        {
            var arg = eventArg;
            var propName = arg.getVar<string>(Arg.VAR_PROP_NAME);
            var value = arg.getVar<object>(Arg.VAR_VALUE_AFTER);
            int life = value is int lifeValue && propName == DEFINE.LIFE ? lifeValue : card.getLife(table.game);
            int curLife = value is int curLifeValue && propName == CARD.CURRENT_LIFE ? curLifeValue : card.getCurrentLife(table.game);
            PropertyNumberColor color = PropertyNumberColor.White;
            //判断是否是红字
            if (curLife < life)
                color = PropertyNumberColor.Red;
            else if (life > card.define.getLife())
                color = PropertyNumberColor.Green;
            return color;
        }
        private PropertyNumberColor getCostColor(BattleTableManager table, ICardData card)
        {
            var arg = eventArg;
            var value = arg.getVar<object>(Arg.VAR_VALUE_AFTER);
            int cost = value is int intValue ? intValue : card.getCost(table.game);
            int defineCost = card.define.getCost();
            var color = PropertyNumberColor.White;
            if (cost < defineCost)
                color = PropertyNumberColor.Green;
            else if (cost > defineCost)
                color = PropertyNumberColor.Red;
            return color;
        }
        public override bool continueNextGroup(IEnumerable<TableAnimation> nextGroup)
        {
            return true;
        }
    }
}
