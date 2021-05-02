using UI;
using TouhouHeartstone;
using TouhouCardEngine;
using System;
using UnityEngine;
using UICard = UI.Card;
using Card = TouhouCardEngine.Card;
using System.Linq;
using BJSYGameCore;
namespace Game
{
    static class UIHelper
    {
        public static void setCard(this UICard ui, CardDefine card, CardSkinData skin)
        {
            ui.servantCardFace.costPropNumber.text.text = card.getCost().ToString();
            ui.spellCardFace.costPropNumber.text.text = card.getCost().ToString();
            ui.itemCardFace.costPropNumber.text.text = card.getCost().ToString();
            ui.servantCardFace.image.sprite = skin.image;
            ui.spellCardFace.image.sprite = skin.image;
            ui.itemCardFace.image.sprite = skin.image;
            ui.servantCardFace.nameText.text = skin.name;
            ui.servantCardFace.subNameText.text = skin.subname;
            LangManager Lang = ui.ui.getManager<LangManager>();
            ui.servantCardFace.raceNameText.text = Lang._((card as ServantCardDefine).tags.FirstOrDefault());
            ui.spellCardFace.nameText.text = skin.name;
            ui.itemCardFace.nameText.text = skin.name;
            ui.servantCardFace.descText.text = skin.desc;
            ui.spellCardFace.descText.text = skin.desc;
            ui.itemCardFace.descText.text = skin.desc;
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