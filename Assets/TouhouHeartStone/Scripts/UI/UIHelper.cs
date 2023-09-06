using UI;
using TouhouHeartstone;
using TouhouCardEngine;
using System;
using UnityEngine;
using UICard = UI.Card;
using Card = TouhouCardEngine.Card;
using System.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
using Game.BattleTable;

namespace Game
{
    static class UIHelper
    {
        public static void setUnknownCard(this UICard ui, bool isFaceUp = true)
        {
            setImage(ui, null);
            setName(ui, StringTable.BUILD_UNKNOW_CARD_NAME);
            setSubname(ui, string.Empty);
            setDesc(ui, string.Empty);
            ui.isFaceup = isFaceUp;
            setCost(ui, "0");
            ui.type = CATEGORY.SERVANT;
            setAttack(ui, "0");
            setLife(ui, "0");
            setCostColor(ui, PropertyNumberColor.White);
            setAttackColor(ui, PropertyNumberColor.White);
            setLifeColor(ui, PropertyNumberColor.White);
            ui.setTag(string.Empty);
        }
        public static void setCardDefine(this UICard ui, CardDefine card, CardSkinData skin, bool isFaceUp = true)
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
            ui.isFaceup = isFaceUp;
            setName(ui, name);
            setSubname(ui, subName);
            setDesc(ui, desc);

            setCost(ui, card.getCost().ToString());
            if (card.type == DEFINE.MASTER)
            {
                ui.type = CATEGORY.MASTER;
            }
            else if (card.type == DEFINE.SERVANT)
            {
                ui.type = CATEGORY.SERVANT;
                ui.servantCardFace.attackPropNumber.text.text = card.getAttack().ToString();
                ui.servantCardFace.lifePropNumber.text.text = card.getLife().ToString();
            }
            else if (card.type == DEFINE.SPELL)
                ui.type = CATEGORY.SPELL;
            else if (card.type == DEFINE.SKILL)
                ui.type = CATEGORY.SKILL;
            else if (card.type == DEFINE.ITEM)
            {
                ui.type = CATEGORY.ITEM;
                ui.itemCardFace.attackPropNumber.text.text = card.getAttack().ToString();
                ui.itemCardFace.lifePropNumber.text.text = card.getLife().ToString();
            }
            else
                throw new NotImplementedException();
            setTag(ui, card.getTags().FirstOrDefault());
        }
        public static async Task setCardDefine(this UICard ui, CardDefine card, CardManager cardManager, bool isFaceUp = true)
        {
            var skin = cardManager.getCardSkin(card);
            ui.setCardDefine(card, skin, isFaceUp);

            Sprite image = skin.image;
            if (!image)
            {
                ui.setImage(null);
                var cardPoolRef = cardManager.getCardPoolReferenceOfCard(card);
                image = await cardManager.loadCardImage(cardPoolRef, card.id);
            }
            ui.setImage(image);
        }
        public static void setCard(this UICard ui, CardEngine game, Card card, CardSkinData skin, bool isFaceUp = true)
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
            ui.isFaceup = isFaceUp;
            setName(ui, name);
            setSubname(ui, subName);
            setDesc(ui, desc);

            setCost(ui, card.getCost(game).ToString());
            if (card.define.type == DEFINE.MASTER)
                ui.type = CATEGORY.MASTER;
            else if (card.define.type == DEFINE.SERVANT)
                ui.type = CATEGORY.SERVANT;
            else if (card.define.type == DEFINE.SPELL)
                ui.type = CATEGORY.SPELL;
            else if (card.define.type == DEFINE.SKILL)
                ui.type = CATEGORY.SKILL;
            else if (card.define.type == DEFINE.ITEM)
                ui.type = CATEGORY.ITEM;
            else
                throw new NotImplementedException();

            var attack = card.getAttack(game);
            var curLife = card.getCurrentLife(game);
            var life = card.getLife(game);
            var cost = card.getCost(game);
            ui.setAttack(attack.ToString());
            ui.setLife(curLife.ToString());

            var costColor =
                cost > card.define.getCost() ? PropertyNumberColor.Red :
                cost < card.define.getCost() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            var attackColor =
                attack > card.define.getAttack() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            var lifeColor =
                curLife < life ? PropertyNumberColor.Red :
                life > card.define.getLife() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            ui.setCostColor(costColor);
            ui.setAttackColor(attackColor);
            ui.setLifeColor(lifeColor);
            setTag(ui, card.getTags(game).FirstOrDefault());
        }
        public static async Task setCard(this UICard ui, CardEngine game, Card card, CardManager cardManager, bool isFaceUp = true)
        {
            var define = card.define;
            var skin = cardManager.getCardSkin(define);
            ui.setCard(game, card, skin, isFaceUp);

            Sprite image = skin.image;
            if (!image)
            {
                ui.setImage(null);
                image = await cardManager.loadImage(skin.imagePath, define.cardPoolId);
            }
            ui.setImage(image);
        }

        public static void setCard(this UICard ui, CardDescriptor card, CardSkinData skin, bool isFaceUp = true)
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
            ui.isFaceup = isFaceUp;
            setName(ui, name);
            setSubname(ui, subName);
            setDesc(ui, description);

            setCost(ui, card.getCost().ToString());
            if (card.define.type == DEFINE.MASTER)
                ui.type = CATEGORY.MASTER;
            else if (card.define.type == DEFINE.SERVANT)
                ui.type = CATEGORY.SERVANT;
            else if (card.define.type == DEFINE.SPELL)
                ui.type = CATEGORY.SPELL;
            else if (card.define.type == DEFINE.SKILL)
                ui.type = CATEGORY.SKILL;
            else if (card.define.type == DEFINE.ITEM)
                ui.type = CATEGORY.ITEM;
            else
                throw new NotImplementedException();

            var attack = card.getAttack();
            var curLife = card.getCurrentLife();
            var life = card.getLife();
            var cost = card.getCost();
            ui.setAttack(attack.ToString());
            ui.setLife(curLife.ToString());

            var costColor =
                cost > card.define.getCost() ? PropertyNumberColor.Red :
                cost < card.define.getCost() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            var attackColor =
                attack > card.define.getAttack() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            var lifeColor =
                curLife < life ? PropertyNumberColor.Red :
                life > card.define.getLife() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            ui.setCostColor(costColor);
            ui.setAttackColor(attackColor);
            ui.setLifeColor(lifeColor);

            setTag(ui, card.getTags().FirstOrDefault());
        }
        public static async Task setCard(this UICard ui, CardDescriptor desc, CardManager cardManager, bool isFaceUp = true)
        {
            var define = desc.define;
            var skin = cardManager.getCardSkin(define);
            ui.setCard(desc, skin, isFaceUp);

            Sprite image = skin.image;
            if (!image)
            {
                ui.setImage(null);
                image = await cardManager.loadImage(skin.imagePath, define.cardPoolId);
            }
            ui.setImage(image);
        }
        public static void setAttack(this UICard ui, string attack)
        {
            ui.servantCardFace.attackPropNumber.text.text = attack;
            ui.itemCardFace.attackPropNumber.text.text = attack;
        }
        public static void setAttackColor(this UICard ui, PropertyNumberColor color)
        {
            ui.servantCardFace.attackPropNumber.setTextColor(color);
            ui.itemCardFace.attackPropNumber.setTextColor(color);
        }
        public static string getAttack(this UICard ui)
        {
            return ui.servantCardFace.attackPropNumber.text.text;
        }
        public static void setLife(this UICard ui, string life)
        {
            ui.servantCardFace.lifePropNumber.text.text = life;
            ui.itemCardFace.lifePropNumber.text.text = life;
        }
        public static void setLifeColor(this UICard ui, PropertyNumberColor color)
        {
            ui.servantCardFace.lifePropNumber.setTextColor(color);
            ui.itemCardFace.lifePropNumber.setTextColor(color);
        }
        public static string getLife(this UICard ui)
        {
            return ui.servantCardFace.lifePropNumber.text.text;
        }
        public static void setDesc(this UICard ui, string desc)
        {
            ui.servantCardFace.descText.text = desc;
            ui.spellCardFace.descText.text = desc;
            ui.itemCardFace.descText.text = desc;
        }
        public static string getDesc(this UICard ui)
        {
            return ui.servantCardFace.descText.text;
        }
        public static void setTag(this UICard ui, string tag)
        {
            CardManager card = ui.ui.getManager<CardManager>();
            
            if (card.tryGetTranslatedTag(tag, out string text))
            {
                ui.servantCardFace.raceNameText.text = text;
            }
            else
                ui.servantCardFace.raceNameText.text = null;
        }
        public static string getTag(this UICard ui)
        {
            return ui.servantCardFace.raceNameText.text;
        }
        public static void setSubname(this UICard ui, string subName)
        {
            ui.servantCardFace.subNameText.text = subName;
        }
        public static string getSubname(this UICard ui)
        {
            return ui.servantCardFace.subNameText.text;
        }
        public static void setName(this UICard ui, string name)
        {
            ui.servantCardFace.nameText.text = name;
            ui.spellCardFace.nameText.text = name;
            ui.itemCardFace.nameText.text = name;
        }
        public static string getName(this UICard ui)
        {
            return ui.servantCardFace.nameText.text;
        }
        public static void setImage(this UICard ui, Sprite sprite)
        {
            ui.servantCardFace.image.sprite = sprite;
            float aspect = sprite != null ? sprite.rect.width / sprite.rect.height : 1;
            ui.servantCardFace.image.GetComponent<AspectRatioFitter>().aspectRatio = aspect;
            ui.spellCardFace.image.sprite = sprite;
            ui.spellCardFace.image.GetComponent<AspectRatioFitter>().aspectRatio = aspect;
            ui.itemCardFace.image.sprite = sprite;
            ui.itemCardFace.image.GetComponent<AspectRatioFitter>().aspectRatio = aspect;
        }
        public static void setCost(this UICard ui, string value)
        {
            ui.servantCardFace.costPropNumber.text.text = value;
            ui.spellCardFace.costPropNumber.text.text = value;
            ui.itemCardFace.costPropNumber.text.text = value;
        }
        public static void setCostColor(this UICard ui, PropertyNumberColor color)
        {
            ui.servantCardFace.costPropNumber.setTextColor(color);
            ui.spellCardFace.costPropNumber.setTextColor(color);
            ui.itemCardFace.costPropNumber.setTextColor(color);
        }
        public static string getCost(this UICard ui)
        {
            return ui.servantCardFace.costPropNumber.text.text;
        }
        public static PropNumber getCostPropNum(this UICard ui, string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                    return ui.servantCardFace.costPropNumber;
                case DEFINE.SPELL:
                    return ui.spellCardFace.costPropNumber;
                case DEFINE.ITEM:
                    return ui.itemCardFace.costPropNumber;
                default:
                    return null;
            }
        }
        public static PropNumber getAttackPropNum(this UICard ui, string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                    return ui.servantCardFace.attackPropNumber;
                case DEFINE.ITEM:
                    return ui.itemCardFace.attackPropNumber;
                default:
                    return null;
            }
        }
        public static PropNumber getLifePropNum(this UICard ui, string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                    return ui.servantCardFace.lifePropNumber;
                case DEFINE.ITEM:
                    return ui.itemCardFace.lifePropNumber;
                default:
                    return null;
            }
        }

        public static PropNumber getCostPropNum(this UICard ui, Card card)
        {
            return ui.getCostPropNum(card.define.type);
        }
        public static PropNumber getAttackPropNum(this UICard ui, Card card)
        {
            return ui.getAttackPropNum(card.define.type);
        }
        public static PropNumber getLifePropNum(this UICard ui, Card card)
        {
            return ui.getLifePropNum(card.define.type);
        }
    }
}