using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(
        typeof(SetGemEventDefine),
        typeof(SetMaxGemEventDefine),
        typeof(MoveCardEventDefine))]
    public class PlayerInitEventDefine : EventDefine
    {
        public PlayerInitEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = EVENTVAR.INIT_MASTER_DEF, type = typeof(CardDefine) },
                new EventVariableInfo() { name = EVENTVAR.INIT_DECK_DEFS, type = typeof(CardDefine[]) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type=typeof(Player) },
                new EventVariableInfo() { name = EVENTVAR.INIT_MASTER_DEF, type = typeof(CardDefine) },
                new EventVariableInfo() { name = EVENTVAR.INIT_DECK_DEFS, type = typeof(CardDefine[]) },
                new EventVariableInfo() { name = EVENTVAR.INIT_MASTER, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.INIT_SKILL, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.INIT_DECK, type = typeof(Card[]) },
            };
        }
        public override async Task execute(IEventArg arg)
        {
            CardEngine game = arg.game;
            Player player = arg.getVar<Player>(EVENTVAR.PLAYER);

            player.addPile(game, new Pile(PILE.MASTER, 1));
            player.addPile(game, new Pile(PILE.SKILL, 1));
            player.addPile(game, new Pile(PILE.DECK));
            player.addPile(game, new Pile(PILE.HAND, maxCount: 10));
            player.addPile(game, new Pile(PILE.INIT, maxCount: 4));
            player.addPile(game, new Pile(PILE.FIELD, maxCount: 7));
            player.addPile(game, new Pile(PILE.GRAVE));
            player.addPile(game, new Pile(PILE.WARP));
            player.addPile(game, new Pile(PILE.HEAD, maxCount: 5));
            player.addPile(game, new Pile(PILE.ITEM, 1));
            player.setIsPrepared(game, false);
            await player.setGem(game, 0);
            await player.setMaxGem(game, 0);
            player.setFatigue(game, 0);

            CardDefine[] startDeck = player.getStartDeck();
            CardDefine masterDef = startDeck[0];
            List<CardDefine> deckCardDefs = startDeck.Skip(1).ToList();
            CardDefine coinDef = game.getDefine(1, ClassicCardID.LuckyCoin);
            for (int i = 1; i < deckCardDefs.Count; i++)
            {
                if (deckCardDefs[i] == null)
                {
                    deckCardDefs.RemoveAt(i);
                    i--;
                }
            }

            CardDefine skillDef = game.getDefine(masterDef.getSkillRef());
            Card masterCard = game.createCard(masterDef);
            await Pile.moveCard(game, masterCard, null, player[PILE.MASTER], 0);
            await masterCard.resetBattlefieldProps(game);

            Card skillCard = game.createCard(skillDef);
            await Pile.moveCard(game, skillCard, null, player[PILE.SKILL], 0);

            var deckCards = deckCardDefs.Select(d => game.createCard(d)).Reverse();
            if (game.getOption().shuffle)
                deckCards = deckCards.shuffle(game);
            Pile deck = player[PILE.DECK];
            foreach (Card deckCard in deckCards)
            {
                await Pile.moveCard(game, deckCard, null, deck, deck.count);
            }
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            Player player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            Card initMaster = eventArg.getVar<Card>(EVENTVAR.INIT_MASTER);
            Card initSkill = eventArg.getVar<Card>(EVENTVAR.INIT_SKILL);
            Card[] initDeck = eventArg.getVar<Card[]>(EVENTVAR.INIT_DECK);

            return new object[] {
                player.name,
                initMaster.getFormatString(),
                initSkill.getFormatString(),
                string.Join('，', Array.ConvertAll(initDeck, c => c.getFormatString())) };
        }
        [TranslateMsg("{0}是玩家，{1}是手牌，{2}是技能，{3}是一串卡牌")]
        const string TEMPLATE = "{0}初始化，初始角色{1}，初始技能{2}，初始套牌{3}";
    }
}
