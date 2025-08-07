using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UICard = Game.BattleTable.TableCard;

namespace Game.BattleTable
{
    static class TableHelper
    {
        public static void setUnknownCard(this UICard ui, CardManager cardManager)
        {
            ui.setImageSprite(cardManager.getDefaultSprite());
            ui.setName(StringTable.BUILD_UNKNOWN_CARD_NAME);
            ui.setSubname(string.Empty);
            ui.setDesc(string.Empty);
            ui.setHighlightColor(CardHighlightColor.None);
            ui.setCostVisible(true);
            ui.setCost(0);
            ui.setType(DEFINE.SERVANT);
            ui.setAttack(0);
            ui.setLife(0);
        }
        public static Task setCardDefine(this UICard ui, CardDefine card, CardManager cardManager)
        {
            var skin = cardManager.getCardSkin(card);
            return ui.setCardDefine(card, skin, cardManager);
        }
        public static async Task setCardDefine(this UICard ui, CardDefine card, CardSkinData skin, CardManager cardManager)
        {
            ICardPoolReference poolRef = cardManager.getCardPoolReferenceOfCard(card);
            string name = cardManager.getLocalizedString(poolRef, skin.nameKey);
            string subName = cardManager.getLocalizedString(poolRef, skin.subnameKey);
            string desc = cardManager.getLocalizedString(poolRef, skin.descKey);
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(desc);

            ui.setCostVisible(!card.hasKeyword(KEYWORD.UNPLAYABLE));
            ui.setCost(card.getCost());
            ui.setAttack(card.getAttack());
            ui.setLife(card.getLife());
            ui.setCostColor(PropertyNumberColor.White);
            ui.setAttackColor(PropertyNumberColor.White);
            ui.setLifeColor(PropertyNumberColor.White);
            ui.setType(card.type);
            ui.setTranslatedTag(cardManager, card.getTags().FirstOrDefault());

            ui.setImageSprite(null);
            ui.setImageSprite(await cardManager.loadCardImage(poolRef, skin));
        }
        public static async Task setCardDefine(this UICard ui, CardDefine card, BattleTableManager table)
        {
            var name = table.getCardName(card);
            var subName = table.getCardSubname(card);
            var desc = table.getCardDesc(card);
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(desc);

            ui.setCostVisible(!card.hasKeyword(KEYWORD.UNPLAYABLE));
            ui.setCost(card.getCost());
            ui.setAttack(card.getAttack());
            ui.setLife(card.getLife());
            ui.setCostColor(PropertyNumberColor.White);
            ui.setAttackColor(PropertyNumberColor.White);
            ui.setLifeColor(PropertyNumberColor.White);
            ui.setType(card.type);
            ui.setTranslatedTag(table.cardManager, card.getTags().FirstOrDefault());

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(card));
        }
        public static async Task setCard(this UICard ui, CardDescriptor desc, BattleTableManager table)
        {
            var define = desc.define;
            var name = table.getCardName(define);
            var subName = table.getCardSubname(define);
            var description = table.getCardDesc(table.game, desc.card);
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(description);


            var cost = desc.getCost();
            var attack = desc.getAttack();
            var currentLife = desc.getCurrentLife();
            var costColor = desc.getCostColor();
            var attackColor = desc.getAttackColor();
            var lifeColor = desc.getLifeColor();

            ui.setUsable(!desc.isModifiedUnplayable);
            ui.setCostVisible(!desc.define.hasKeyword(KEYWORD.UNPLAYABLE));
            ui.setCost(cost);
            ui.setAttack(attack);
            ui.setLife(currentLife);
            ui.setCostColor(costColor);
            ui.setAttackColor(attackColor);
            ui.setLifeColor(lifeColor);
            ui.setType(desc.define.type);
            ui.setTranslatedTag(table.cardManager, desc.getTags().FirstOrDefault());

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(define));
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