using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;
using UICard = Game.BattleTable.TableCard;

namespace Game.BattleTable
{
    static class TableHelper
    {
        public static void setUnknownCard(this UICard ui, CardManager cardManager)
        {
            ui.setImageSprite(cardManager.getDefaultSprite());
            ui.setName(StringTable.BUILD_UNKNOW_CARD_NAME);
            ui.setSubname(string.Empty);
            ui.setDesc(string.Empty);
            ui.setHighlightColor(CardHighlightColor.None);
            ui.setCostVisible(true);
            ui.setCost(0);
            ui.setType(DEFINE.SERVANT);
            ui.setAttack(0);
            ui.setLife(0);
        }


        public static void setCardDefine(this UICard ui, CardDefine card, CardSkinData skin, CardManager cardManager)
        {
            string name = StringTable.BUILD_UNKNOW_CARD_NAME;
            string subName = string.Empty;
            string desc = string.Empty;
            if (skin != null)
            {
                name = skin.name;
                subName = skin.subname;
                desc = skin.desc;
            }
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(desc);

            ui.setHighlightColor(CardHighlightColor.None);
            ui.setCostVisible(!card.hasKeyword(KEYWORD.UNPLAYABLE));
            ui.setCost(card.getCost());
            ui.setAttack(card.getAttack());
            ui.setLife(card.getLife());
            ui.setCostColor(PropertyNumberColor.White);
            ui.setAttackColor(PropertyNumberColor.White);
            ui.setLifeColor(PropertyNumberColor.White);
            ui.setType(card.type);
            ui.setTranslatedTag(cardManager, card.getTags().FirstOrDefault());
        }
        public static void setCard(this UICard ui, CardEngine game, Card card, CardSkinData skin, BattleTableManager table)
        {
            string name = StringTable.BUILD_UNKNOW_CARD_NAME;
            string subName = string.Empty;
            string desc = string.Empty;
            if (skin != null)
            {
                name = skin.name;
                subName = skin.subname;
                desc = skin.desc;
            }
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(desc);

            var cost = card.getCost(game);
            var attack = card.getAttack(game);
            var currentLife = card.getCurrentLife(game);
            var costColor = card.getCostColor(game);
            var attackColor = card.getAttackColor(game);
            var lifeColor = card.getLifeColor(game);
            var highlightColor = card.isModifiedUnplayable(game) ? CardHighlightColor.Red : CardHighlightColor.None;

            ui.setHighlightColor(highlightColor);
            ui.setCostVisible(!card.define.hasKeyword(KEYWORD.UNPLAYABLE));
            ui.setCost(cost);
            ui.setAttack(attack);
            ui.setLife(currentLife);
            ui.setCostColor(costColor);
            ui.setAttackColor(attackColor);
            ui.setLifeColor(lifeColor);
            ui.setType(card.define.type);
            ui.setTranslatedTag(table.cardManager, card.getTags(game).FirstOrDefault());
        }
        public static void setCard(this UICard ui, CardDescriptor desc, CardSkinData skin, BattleTableManager table)
        {
            string name = StringTable.BUILD_UNKNOW_CARD_NAME;
            string subName = string.Empty;
            string description = string.Empty;
            if (skin != null)
            {
                name = skin.name;
                subName = skin.subname;
                description = skin.desc;
            }
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(description);


            var cost = desc.getCost();
            var attack = desc.getAttack();
            var currentLife = desc.getCurrentLife();
            var costColor = desc.getCostColor();
            var attackColor = desc.getAttackColor();
            var lifeColor = desc.getLifeColor();
            var highlightColor = desc.isModifiedUnplayable ? CardHighlightColor.Red : CardHighlightColor.None;

            ui.setHighlightColor(highlightColor);
            ui.setCostVisible(!desc.define.hasKeyword(KEYWORD.UNPLAYABLE));
            ui.setCost(cost);
            ui.setAttack(attack);
            ui.setLife(currentLife);
            ui.setCostColor(costColor);
            ui.setAttackColor(attackColor);
            ui.setLifeColor(lifeColor);
            ui.setType(desc.define.type);
            ui.setTranslatedTag(table.cardManager, desc.getTags().FirstOrDefault());
        }

        public static async Task setCardDefine(this UICard ui, CardDefine card, BattleTableManager table)
        {
            var skin = table.getCardSkin(card);
            ui.setCardDefine(card, skin, table.cardManager);

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(card));
        }
        public static async Task setCard(this UICard ui, CardEngine game, Card card, BattleTableManager table)
        {
            var define = card.define;
            var skin = table.getCardSkin(define);
            ui.setCard(game, card, skin, table);

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(define));
        }
        public static async Task setCard(this UICard ui, CardDescriptor desc, BattleTableManager table)
        {
            var define = desc.define;
            var skin = table.getCardSkin(define);
            ui.setCard(desc, skin, table);

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(define));
        }
        public static PropertyNumberColor getCostColor(this Card card, IGame game)
        {
            var cost = card.getCost(game);
            var defineCost = card.define.getCost();
            if (cost < defineCost)
                return PropertyNumberColor.Green;
            else if (cost > defineCost)
                return PropertyNumberColor.Red;
            return PropertyNumberColor.White;
        }
        public static PropertyNumberColor getAttackColor(this Card card, IGame game)
        {
            var attack = card.getAttack(game);
            var defineAttack = card.define.getAttack();
            if (attack > defineAttack)
                return PropertyNumberColor.Green;
            return PropertyNumberColor.White;
        }
        public static PropertyNumberColor getLifeColor(this Card card, IGame game)
        {
            var currentLife = card.getCurrentLife(game);
            var life = card.getLife(game);
            var defineLife = card.define.getLife();
            if (currentLife < life)
                return PropertyNumberColor.Red;
            else if (life > defineLife)
                return PropertyNumberColor.Green;
            return PropertyNumberColor.White;
        }
        public static PropertyNumberColor getCostColor(this CardDescriptor card)
        {
            var cost = card.getCost();
            var defineCost = card.define.getCost();
            if (cost < defineCost)
                return PropertyNumberColor.Green;
            else if (cost > defineCost)
                return PropertyNumberColor.Red;
            return PropertyNumberColor.White;
        }
        public static PropertyNumberColor getAttackColor(this CardDescriptor card)
        {
            var attack = card.getAttack();
            var defineAttack = card.define.getAttack();
            if (attack > defineAttack)
                return PropertyNumberColor.Green;
            return PropertyNumberColor.White;
        }
        public static PropertyNumberColor getLifeColor(this CardDescriptor card)
        {
            var currentLife = card.getCurrentLife();
            var life = card.getLife();
            var defineLife = card.define.getLife();
            if (currentLife < life)
                return PropertyNumberColor.Red;
            else if (life > defineLife)
                return PropertyNumberColor.Green;
            return PropertyNumberColor.White;
        }
        public static void setTranslatedTag(this UICard ui, CardManager cardManager, string tag)
        {
            string name = tag;
            if (tag != null && cardManager.tryGetTranslatedTag(tag, out string text))
            {
                name = text;
            }
            ui.setTag(name);
        }
    }
}