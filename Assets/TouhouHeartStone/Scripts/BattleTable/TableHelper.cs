using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using Card = TouhouCardEngine.Card;
using UICard = Game.BattleTable.TableCard;

namespace Game.BattleTable
{
    static class TableHelper
    {
        public static void setUnknownCard(this UICard ui, CardManager cardManager, bool isFaceUp = true)
        {
            ui.setImageSprite(cardManager.getDefaultSprite());
            ui.setName(StringTable.BUILD_UNKNOW_CARD_NAME);
            ui.setSubname(string.Empty);
            ui.setDesc(string.Empty);
            ui.setFaceup(isFaceUp);
            ui.setCost(0);
            ui.setType(DEFINE.SERVANT);
            ui.setAttack(0);
            ui.setLife(0);
        }


        public static void setCardDefine(this UICard ui, CardDefine card, CardSkinData skin, BattleTableManager table, bool isFaceUp = true)
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
            ui.setFaceup(isFaceUp);
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(desc);

            ui.setCost(card.getCost());
            ui.setAttack(card.getAttack());
            ui.setLife(card.getLife());
            ui.setType(card.type);
            ui.setTranslatedTag(table.cardManager, card.getTags().FirstOrDefault());
        }
        public static void setCard(this UICard ui, CardEngine game, Card card, CardSkinData skin, BattleTableManager table, bool isFaceUp = true)
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
            ui.setFaceup(isFaceUp);
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(desc);

            ui.setCost(card.getCost(game));
            ui.setAttack(card.getAttack(game));
            ui.setLife(card.getLife(game));
            ui.setType(card.define.type);
            ui.setTranslatedTag(table.cardManager, card.getTags(game).FirstOrDefault());
        }
        public static void setCard(this UICard ui, CardDescriptor desc, CardSkinData skin, BattleTableManager table, bool isFaceUp = true)
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
            ui.setFaceup(isFaceUp);
            ui.setName(name);
            ui.setSubname(subName);
            ui.setDesc(description);

            ui.setCost(desc.getCost());
            ui.setAttack(desc.getAttack());
            ui.setLife(desc.getLife());
            ui.setType(desc.define.type);
            ui.setTranslatedTag(table.cardManager, desc.getTags().FirstOrDefault());
        }

        public static async Task setCardDefine(this UICard ui, CardDefine card, BattleTableManager table, bool isFaceUp = true)
        {
            var skin = table.getCardSkin(card);
            ui.setCardDefine(card, skin, table, isFaceUp);

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(card));
        }
        public static async Task setCard(this UICard ui, CardEngine game, Card card, BattleTableManager table, bool isFaceUp = true)
        {
            var define = card.define;
            var skin = table.getCardSkin(define);
            ui.setCard(game, card, skin, table, isFaceUp);

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(define));
        }
        public static async Task setCard(this UICard ui, CardDescriptor desc, BattleTableManager table, bool isFaceUp = true)
        {
            var define = desc.define;
            var skin = table.getCardSkin(define);
            ui.setCard(desc, skin, table, isFaceUp);

            ui.setImageSprite(null);
            ui.setImageSprite(await table.loadCardImage(define));
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