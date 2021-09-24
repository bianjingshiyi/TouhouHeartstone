using UI;
using TouhouHeartstone;
using TouhouCardEngine;
using System;
using UnityEngine;
using UICard = UI.Card;
using Card = TouhouCardEngine.Card;
using System.Linq;
using BJSYGameCore;
using UnityEngine.UI;

namespace Game
{
    static class UIHelper
    {
        public static void setCard(this UICard ui, CardDefine card, CardSkinData skin)
        {
            setCost(ui, card.getCost().ToString());
            setImage(ui, skin.image);
            setName(ui, skin.name);
            setSubname(ui, skin.subname);
            if (card is ServantCardDefine servantCardDefine)
            {
                setTag(ui, servantCardDefine.tags.FirstOrDefault());
            }
            else
            {
                setTag(ui, card.getTags().FirstOrDefault());
            }
            setDesc(ui, skin.desc);
            ui.isFaceup = true;

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
        }
        public static void setAttack(this UICard ui, string attack)
        {
            ui.servantCardFace.attackPropNumber.text.text = attack;
            ui.itemCardFace.attackPropNumber.text.text = attack;
        }
        public static void setLife(this UICard ui, string life)
        {
            ui.servantCardFace.lifePropNumber.text.text = life;
            ui.itemCardFace.lifePropNumber.text.text = life;
        }
        public static void setDesc(this UICard ui, string desc)
        {
            ui.servantCardFace.descText.text = desc;
            ui.spellCardFace.descText.text = desc;
            ui.itemCardFace.descText.text = desc;
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

        public static void setSubname(this UICard ui, string subName)
        {
            ui.servantCardFace.subNameText.text = subName;
        }
        public static void setName(this UICard ui, string name)
        {
            ui.servantCardFace.nameText.text = name;
            ui.spellCardFace.nameText.text = name;
            ui.itemCardFace.nameText.text = name;
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