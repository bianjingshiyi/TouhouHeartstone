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
            {
                ui.type = CATEGORY.MASTER;
                ui.setLife("30");
            }
            else if (card.define.type == DEFINE.SERVANT)
            {
                ui.type = CATEGORY.SERVANT;
                ui.servantCardFace.attackPropNumber.text.text = card.getAttack(game).ToString();
                ui.servantCardFace.lifePropNumber.text.text = card.getLife(game).ToString();
            }
            else if (card.define.type == DEFINE.SPELL)
                ui.type = CATEGORY.SPELL;
            else if (card.define.type == DEFINE.SKILL)
                ui.type = CATEGORY.SKILL;
            else if (card.define.type == DEFINE.ITEM)
            {
                ui.type = CATEGORY.ITEM;
                ui.itemCardFace.attackPropNumber.text.text = card.getAttack(game).ToString();
                ui.itemCardFace.lifePropNumber.text.text = card.getLife(game).ToString();
            }
            else
                throw new NotImplementedException();
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
                image = await cardManager.loadImage(skin.imagePath, define.cardPoolId);
            }
            ui.setImage(image);
        }
        public static void setAttack(this UICard ui, string attack)
        {
            ui.servantCardFace.attackPropNumber.text.text = attack;
            ui.itemCardFace.attackPropNumber.text.text = attack;
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
            LangManager Lang = ui.ui.getManager<LangManager>();
            if (tag != null)
            {
                if (ui.ui.getManager<TableManager>().TranslateTagDic.TryGetValue(tag, out string text))
                {
                    ui.servantCardFace.raceNameText.text = Lang._(text);
                }
                else
                {
                    ui.servantCardFace.raceNameText.text = tag;
                }
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
        public static string getCost(this UICard ui)
        {
            return ui.servantCardFace.costPropNumber.text.text;
        }
        public static PropNumber getCostPropNum(this UICard ui, Card card)
        {
            switch (card.define.type)
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
        public static PropNumber getAttackPropNum(this UICard ui, Card card)
        {
            switch (card.define.type)
            {
                case DEFINE.SERVANT:
                    return ui.servantCardFace.attackPropNumber;
                case DEFINE.ITEM:
                    return ui.itemCardFace.attackPropNumber;
                default:
                    return null;
            }
        }
        public static PropNumber getLifePropNum(this UICard ui, Card card)
        {
            switch (card.define.type)
            {
                case DEFINE.SERVANT:
                    return ui.servantCardFace.lifePropNumber;
                case DEFINE.ITEM:
                    return ui.itemCardFace.lifePropNumber;
                default:
                    return null;
            }
        }
        public static RectTransform getRoot(this ServantItem servant)
        {
            return servant.getChild("Servant");
        }
    }
}