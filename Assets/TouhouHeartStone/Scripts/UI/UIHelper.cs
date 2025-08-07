using System.Linq;
using System.Threading.Tasks;
using Game.BattleTable;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UICard = UI.Card;

namespace Game
{
    static class UIHelper
    {
        public static UICardType parseUICardType(string type)
        {
            switch (type)
            {
                case DEFINE.ITEM:
                    return UICardType.Item;
                case DEFINE.SPELL:
                case DEFINE.SKILL:
                    return UICardType.Spell;
                default:
                    return UICardType.Servant;
            }
        }
        public static void updateCard(this UICard ui, UICardViewData viewData)
        {
            ui.setType(viewData.type);
            ui.setFaceup(viewData.isFaceUp);
            ui.setCostVisible(viewData.costVisible);
            ui.setCost(viewData.cost);
            ui.setAttack(viewData.attack);
            ui.setLife(viewData.life);
            ui.setTag(viewData.tag);
            ui.setName(viewData.name);
            ui.setSubname(viewData.subname);
            ui.setDesc(viewData.desc);
        }
        public static void setUnknownCard(this UICard ui, bool isFaceUp = true)
        {
            var viewData = new UICardViewData()
            {
                type = UICardType.Servant,
                isFaceUp = isFaceUp,
                costVisible = true,
                cost = "0",
                attack = "0",
                life = "0",
                costColor = PropertyNumberColor.White,
                attackColor = PropertyNumberColor.White,
                lifeColor = PropertyNumberColor.White,
                tag = string.Empty,
                name = StringTable.BUILD_UNKNOWN_CARD_NAME,
                subname = string.Empty,
                desc = string.Empty,
            };
            ui.updateCard(viewData);
        }
        public static UICardViewData getCardDefineViewData(CardDefine card, CardManager cardManager)
        {
            var skin = cardManager.getCardSkin(card);
            return getCardDefineViewData(card, skin, cardManager);
        }
        public static UICardViewData getCardDefineViewData(CardDefine card, CardSkinData skin, CardManager cardManager)
        {
            var poolRef = cardManager.getCardPoolReferenceOfCard(card);
            string name = StringTable.BUILD_UNKNOWN_CARD_NAME;
            string subName = string.Empty;
            string desc = string.Empty;
            if (skin != null)
            {
                name = cardManager.getCardName(poolRef, skin);
                subName = cardManager.getCardSubname(poolRef, skin);
                desc = cardManager.getCardDesc(poolRef, skin);
            }
            return new UICardViewData()
            {
                type = parseUICardType(card.type),
                isFaceUp = true,
                costVisible = !card.hasKeyword(KEYWORD.UNPLAYABLE),
                cost = card.getCost().ToString(),
                attack = card.getAttack().ToString(),
                life = card.getLife().ToString(),
                tag = card.getTags().FirstOrDefault(),
                name = name,
                subname = subName,
                desc = desc,
            };
        }
        public static Task setCardDefine(this UICard ui, CardDefine card, CardManager cardManager, bool isFaceUp = true)
        {
            var skin = cardManager.getCardSkin(card);
            return ui.setCardDefine(card, skin, cardManager, isFaceUp);
        }
        public static async Task setCardDefine(this UICard ui, CardDefine card, CardSkinData skin, CardManager cardManager, bool isFaceUp = true)
        {
            var poolRef = cardManager.getCardPoolReferenceOfCard(card);
            var viewData = getCardDefineViewData(card, skin, cardManager);
            viewData.isFaceUp = isFaceUp;
            ui.updateCard(viewData);
            // 先设置卡牌定义再加载卡图，不然卡框会和卡图一起被突然改变，看起来会有些别扭
            ui.setImage(await cardManager.loadCardImage(poolRef, skin));
        }
        public static async Task setCardDefine(this UICard ui, CardDefine card, BattleTableManager table, bool isFaceUp = true)
        {
            var viewData = new UICardViewData()
            {
                type = parseUICardType(card.type),
                isFaceUp = isFaceUp,
                costVisible = !card.hasKeyword(KEYWORD.UNPLAYABLE),
                cost = card.getCost().ToString(),
                attack = card.getAttack().ToString(),
                life = card.getLife().ToString(),
                tag = card.getTags().FirstOrDefault(),
                name = table.getCardName(card),
                subname = table.getCardSubname(card),
                desc = table.getCardDesc(card),
            };
            ui.updateCard(viewData);

            // 先设置卡牌定义再加载卡图，不然卡框会和卡图一起被突然改变，看起来会有些别扭
            ui.setImage(await table.loadCardImage(card));
        }
        public static void setTypeByName(this UICard ui, string typeName)
        {
            var type = parseUICardType(typeName);
            ui.setType(type);
        }
        public static async Task setCard(this UICard ui, CardDescriptor desc, BattleTableManager table, bool isFaceUp = true)
        {
            var define = desc.define;
            var attack = desc.getAttack();
            var curLife = desc.getCurrentLife();
            var life = desc.getLife();
            var cost = desc.getCost();
            var costColor =
                cost > define.getCost() ? PropertyNumberColor.Red :
                cost < define.getCost() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            var attackColor =
                attack > define.getAttack() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;
            var lifeColor =
                curLife < life ? PropertyNumberColor.Red :
                life > define.getLife() ? PropertyNumberColor.Green :
                PropertyNumberColor.White;

            var viewData = new UICardViewData()
            {
                type = parseUICardType(define.type),
                isFaceUp = isFaceUp,
                costVisible = !define.hasKeyword(KEYWORD.UNPLAYABLE),

                cost = cost.ToString(),
                attack = attack.ToString(),
                life = curLife.ToString(),

                costColor = costColor,
                attackColor = attackColor,
                lifeColor = lifeColor,

                tag = desc.getTags().FirstOrDefault(),
                name = table.getCardName(desc.define),
                subname = table.getCardSubname(desc.define),
                desc = table.getCardDesc(desc.define)
            };
            ui.updateCard(viewData);

            ui.setImage(await table.loadCardImage(define));
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

            ui.servantCardFace.raceNameText.text = card.tryGetTranslatedTag(tag, out string text) ? text : tag;
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
        public static void setCostVisible(this UICard ui, bool value)
        {
            ui.servantCardFace.costPropNumber.text.gameObject.SetActive(value);
            ui.spellCardFace.costPropNumber.text.gameObject.SetActive(value);
            ui.itemCardFace.costPropNumber.text.gameObject.SetActive(value);
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
    }
}