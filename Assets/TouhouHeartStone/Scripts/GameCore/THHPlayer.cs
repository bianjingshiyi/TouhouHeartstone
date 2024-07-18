using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using UnityEngine;
using MukioI18n;

namespace TouhouHeartstone
{
    public static class THHPlayer
    {
        #region 公有方法

        #region 牌堆
        public static Card getSkill(this Player player) => player.getPile(PILE.SKILL)[0];
        public static Pile getDeck(this Player player) => player.getPile(PILE.DECK);
        public static Pile getInit(this Player player) => player.getPile(PILE.INIT);
        public static Pile getHand(this Player player) => player.getPile(PILE.HAND);
        public static Pile getField(this Player player) => player.getPile(PILE.FIELD);
        public static Pile getGrave(this Player player) => player.getPile(PILE.GRAVE);
        /// <summary>
        /// 正在使用的法术卡都会被置入亚空间
        /// </summary>
        public static Pile getWarp(this Player player) => player.getPile(PILE.WARP);
        /// <summary>
        /// 头顶的五个玩意
        /// </summary>
        public static Pile getHead(this Player player) => player.getPile(PILE.HEAD);
        public static Card getItem(this Player player)
        {
            return player[PILE.ITEM].Any() ? player[PILE.ITEM][0] : null;
        }
        public static bool removePlayerPile(this Player player, CardEngine game, Pile pile)
        {
            if (pile?.name == null || THHPile.vanillaPileNames.Contains(pile?.name))
                return false;
            return player.removePile(game, pile);
        }
        #endregion

        #region 设置玩家属性
        public static void setIsPrepared(this Player player, CardEngine game, bool value) => player.setProp(game, PLAYER.ISPREPARED, value);
        public static bool getIsPrepared(this Player player) => player.getProp<bool>(PLAYER.ISPREPARED);
        public static int getGem(this Player player) => player.getProp<int>(PLAYER.GEM);
        public static int getMaxGem(this Player player) => player.getProp<int>(PLAYER.MAXGEM);
        public static Task<EventArg> setGem(this Player player, CardEngine game, int value)
        {
            return SetGemEventDefine.doEvent(game, player, value);
        }
        public static Task<EventArg> setMaxGem(this Player player, CardEngine game, int value)
        {
            value = Math.Clamp(value, 0, 10);
            return SetMaxGemEventDefine.doEvent(game, player, value);
        }
        public static void setStartDeck(this Player player, CardEngine game, CardDefine[] value)
        {
            player.setProp(game, PLAYER.START_DECK, value);
        }
        public static CardDefine[] getStartDeck(this Player player)
        {
            return player.getProp<CardDefine[]>(PLAYER.START_DECK);
        }
        #endregion

        #region 抽牌
        public static Task draw(this Player player, CardEngine game, out EventArg drawEvent, out EventArg burnEvent, out EventArg fatigueEvent)
        {
            if (player.getDeck().count < 1)//无牌可抽，疲劳！
            {
                drawEvent = null;
                burnEvent = null;
                fatigueEvent = FatigueEventDefine.newEvent(game, player);
                return game.triggers.doEvent(fatigueEvent);
            }
            else
            {
                fatigueEvent = null;
                return player.draw(game, player.getDeck().top, out drawEvent, out burnEvent);
            }
        }
        public static Task draw(this Player player, CardEngine game, out EventArg drawEvent)
        {
            return player.draw(game, out drawEvent, out _, out _);
        }
        public static Task draw(this Player player, CardEngine game, out EventArg drawEvent, out EventArg burnEvent)
        {
            return player.draw(game, out drawEvent, out burnEvent, out _);
        }
        public static Task draw(this Player player, CardEngine game)
        {
            return player.draw(game, out _, out _, out _);
        }
        public static Task draw(this Player player, CardEngine game, Card card, out EventArg drawEvent, out EventArg burnEvent)
        {
            if (!player.getDeck().Contains(card))
            {
                drawEvent = null;
                burnEvent = null;
                return Task.CompletedTask;
            }
            if (player.getHand().count >= player.getHand().maxCount)
            {
                drawEvent = null;
                burnEvent = BurnEventDefine.newEvent(game, player, card);
                return game.triggers.doEvent(burnEvent);
            }
            else
            {
                drawEvent = DrawEventDefine.newEvent(game, player, card);
                burnEvent = null;
                return game.triggers.doEvent(drawEvent);
            }
        }
        public static Task draw(this Player player, CardEngine game, Card card)
        {
            return player.draw(game, card, out _, out _);
        }
        public static Task draw(this Player player, CardEngine game, Card card, out EventArg drawEvent)
        {
            return player.draw(game, card, out drawEvent, out _);
        }
        /// <summary>
        /// 抽一张符合条件的牌，如果没有符合条件的牌则抽牌不会发生，返回事件为空。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task draw(this Player player, CardEngine game, Func<Card, bool> filter, out EventArg drawEvent)
        {
            var cards = player.getDeck().Where(filter);
            if (cards.Count() > 0)
            {
                var card = cards.random(game);
                return player.draw(game, card, out drawEvent);
            }
            else
            {
                drawEvent = null;
                return Task.CompletedTask;
            }
        }
        #endregion

        #region 疲劳
        public static void setFatigue(this Player player, CardEngine game, int value) => player.setProp(game, PLAYER.FATIGUE, value);
        public static int getFatigue(this Player player) => player.getProp<int>(PLAYER.FATIGUE);
        public static Task<EventArg> fatigue(this Player player, CardEngine game)
        {
            return FatigueEventDefine.doEvent(game, player);
        }
        #endregion

        #region 自动施放
        public static Task<EventArg> autocast(this Player player, CardEngine game, Card card)
        {
            var targets = card.getAllPossibleTargetSets(game).random(game);
            return AutocastEventDefine.doEvent(game, player, card, targets);
        }
        #endregion

        #region 选择卡牌 
        public static async Task<EventArg> chooseOne(this Player player, CardEngine game, IEnumerable<CardDefine> options, bool isShow = true, int count = 3, string title = "选择一项")
        {
            if (options.Count() < 1)
                return null;
            return await SelectCardDefineEventDefine.doEvent(game, player, options.ToArray(), count, title, isShow);
        }
        public static async Task<EventArg> searchPile(this Player player, CardEngine game, IEnumerable<Card> cards, bool isShow = false, int count = 3, string title = "选择一项")
        {
            if (cards.Count() < 1)
                return null;
            return await SelectCardEventDefine.doEvent(game, player, cards.ToArray(), count, title, isShow);
        }


        #endregion

        #region 指令
        public static Task cmdInitReplace(this Player player, CardEngine game, params Card[] cards)
        {
            InitReplaceResponse response = new InitReplaceResponse()
            {
                playerId = player.id,
                cardsId = cards.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(player.id, response);
            return game.answers.answer(player.id, response);
        }
        public static Task cmdUse(this Player player, CardEngine game, Card card, int position = 0, params Card[] targets)
        {
            var response = new UseResponse()
            {
                cardId = card.id,
                position = position,
                targetsId = targets.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(player.id, response);
            return game.answers.answer(player.id, response);
        }
        public static Task cmdTurnEnd(this Player player, CardEngine game)
        {
            var resp = new TurnEndResponse() { };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdAttack(this Player player, CardEngine game, Card card, Card target)
        {
            var resp = new AttackResponse()
            {
                cardId = card.id,
                targetId = target.id
            };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdSurrender(this Player player, CardEngine game)
        {
            var resp = new SurrenderResponse();
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdDiscover(this Player player, CardEngine game, int cardId)
        {
            var resp = new DiscoverResponse()
            {
                cardId = cardId
            };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdDiscover(this Player player, CardEngine game, DefineReference cardDefRef)
        {
            var resp = new DiscoverResponse() { cardDefRef = cardDefRef };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        #endregion

        #region 道具
        public static Task<EventArg> equip(this Player player, CardEngine game, Pile from, Card item)
        {
            if (from != null && !from.Contains(item)) // 源牌堆不包含该卡牌。
                return null;
            return EquipItemEventDefine.doEvent(game, player, from, item);
        }
        #endregion

        #region 移动卡牌
        public static Task<EventArg> tryEnterField(this Player player, CardEngine game, Pile from, Card card, int position)
        {
            if (card == null || !card.isServant() || !Pile.canMove(card, from, player.getField()))
                return Task.FromResult(default(EventArg));
            return EnterFieldEventDefine.doEvent(game, player, from, card, position);
        }
        public static async Task<bool> tryEnterHead(this Player player, CardEngine game, Pile from, Card card)
        {
            var eventArg = await card.moveToHead(game, from, player?.getHead(), -1);
            return eventArg != null;
        }
        public static Task<EventArg> createToken(this Player player, CardEngine game, CardDefine define, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (define == null)
                throw new ArgumentNullException(nameof(define));
            if (player.getField().count >= player.getField().maxCount)
                return Task.FromResult(default(EventArg));
            if (position < 0)
                position = player.getField().count;
            var card = game.createCard(define);
            return player.tryEnterField(game, null, card, position);
        }
        /// <summary>
        /// 复生，墓地到场上
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task<EventArg> reborn(this Player player, CardEngine game, Card card, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (card == null || card.pile.name != PILE.GRAVE)
                return Task.FromResult(default(EventArg));
            if (position < 0)
                position = player.getField().count;
            return RebornEventDefine.doEvent(game, player, card, position);
        }
        public static async Task<Card> shuffleCardToDeck(this Player player, CardEngine game, CardDefine define)
        {
            Card card = game.createCard(define);
            await player.getDeck().insert(game, card, game.randomInt(0, player.getDeck().count));
            return card;
        }
        public async static Task<Card> addCardToHand(this Player player, CardEngine game, CardDefine cardDefine)
        {
            if (player.getHand().isFull)
                return null;
            Card card = game.createCard(cardDefine);
            await player.getHand().add(game, card);
            return card;
        }
        public static Task<Card> addRandomCardToHand(this Player player, CardEngine game, IEnumerable<CardDefine> pool)
        {
            return player.addCardToHand(game, pool.random(game));
        }
        public static Task<EventArg> randomDiscard(this Player player, CardEngine game, int count = 1, Func<Card, bool> filter = null)
        {
            var cards = filter == null ? player.getHand() : player.getHand().Where(c => filter(c));
            if (cards.Count() < 1)
                return default;
            var discards = cards;
            if (cards.Count() > count)
                discards = cards.randomTake(game, count);
            return discards.discard(game);
        }
        #endregion

        #region 信仰对决
        /// <summary>
        /// 揭示对决
        /// 双方揭示牌库里的一张牌，法力消耗大的胜利
        /// </summary>
        /// <param name="game"></param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static Task<EventArg> revealDuel(this Player player, CardEngine game, int define)
        {
            return player.revealDuel(game, getRevealFilterByDefine(define));
        }

        /// <summary>
        /// 揭示对决
        /// 双方揭示牌库里的一张牌，法力消耗大的胜利
        /// </summary>
        /// <param name="game"></param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static Task<EventArg> revealDuel(this Player player, CardEngine game, Predicate<Card> filter = null)
        {
            var oppo = game.getOpponent(player);
            var selfCards = player.getDeck().Where(c => filter != null ? filter.Invoke(c) : true).ToArray();
            var oppoCards = oppo.getDeck().Where(c => filter != null ? filter.Invoke(c) : true).ToArray();
            return RevealDuelEventDefine.doEvent(game, player, selfCards, oppoCards);
        }
        private static Predicate<Card> getRevealFilterByDefine(int define)
        {
            bool filter(Card card)
            {
                switch (define)
                {
                    case 0:
                        return true;
                    case 1:
                        return card.isSpell();
                    case 2:
                        return card.isServant();
                    case 3:
                        return card.isItem();
                    default:
                        return false;
                }
            }
            return filter;
        }

        #endregion 信仰对决

        #region 法术伤害
        public static int getSpellDamage(this Player player, CardEngine game, int baseDamage, Card source)
        {
            var spellDamage = player.getSpellDamage(game, baseDamage);
            if (source != null && source.isSpell())
            {
                spellDamage += source.getSpellDamage(game);
            }
            return spellDamage;
        }
        public static int getSpellDamage(this Player player, CardEngine game, int baseDamage)
        {
            int result = baseDamage;
            foreach (var servant in player.getField())
            {
                result += servant.getSpellDamage(game);
            }
            result += player.getItem()?.getSpellDamage(game) ?? 0;
            result += player.getSkill()?.getSpellDamage(game) ?? 0;
            result += player.getMaster()?.getSpellDamage(game) ?? 0;
            return result;
        }
        #endregion

        #region 杂项
        public static async Task updateSecrets(this Player player, CardEngine game)
        {
            foreach (var secret in player.getHead()) // 位于延迟区。
            {
                await secret.updateSecret(game);
            }
        }
        public static bool canAddToHead(this Player player, CardEngine game, Card card, Pile from, Pile to, out string info)
        {
            info = null;
            if (!card.isSpell())
                return false;
            if (card.isTrap(game))
            {
                return player.canAddTrap(card, from, to, out info);
            }
            else if (card.hasKeyword(game, KEYWORD.MISSION))
            {
                return player.canAddMission(card, from, to, out info);
            }
            return true;
        }
        public static bool canAddTrap(this Player player, Card card, Pile from, Pile to, out string info)
        {
            var head = player.getHead();
            if (head.isFull && from != to)
            {
                info = NO_SPACE_FOR_TRAP;
                return false;
            }
            if (head.Any(c => c.define == card.define))
            {
                info = DUPLICATED_TRAP;
                return false;
            }
            info = null;
            return true;
        }
        public static bool canAddMission(this Player player, Card card, Pile from, Pile to, out string info)
        {
            var head = player.getHead();
            if (head.isFull && from != to)
            {
                info = NO_SPACE_FOR_TRAP;
                return false;
            }
            if (head.Any(c => c.define.hasKeyword(KEYWORD.MISSION)))
            {
                info = ONLY_ONE_MISSION;
                return false;
            }
            info = null;
            return true;
        }
        /// <summary>
        /// 尝试使用一张卡牌
        /// </summary>
        /// <param name="game">游戏环境</param>
        /// <param name="card">使用的卡牌</param>
        /// <param name="position">随从卡放置在场上的位置</param>
        /// <param name="targets">卡牌使用目标</param>
        /// <returns></returns>
        public static async Task<bool> tryUse(this Player player, CardEngine game, Card card, int position, params Card[] targets)
        {
            if (!card.isUsable(game, player, out _))
                //卡牌不可用
                return false;
            var cost = card.getCost(game);
            await player.setGem(game, player.getGem() - cost);//消耗法力值
            await UseEventDefine.doEvent(game, player, card, position, targets, cost);
            return true;
        }
        public static Card getMaster(this Player player) => player.getPile(PILE.MASTER)[0];
        /// <summary>
        /// 找到符合条件的卡片，无论它在哪里。
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks>但是其实并不会真的哪里都去找啦</remarks>
        public static Card[] findCardsWherever(this Player player, CardEngine game, Func<Card, bool> filter, PileFlag pile = PileFlag.self | PileFlag.field | PileFlag.hand | PileFlag.grave | PileFlag.deck)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            List<Card> cardList = new List<Card>();
            foreach (var p in pile.getPiles(game, player))
            {
                cardList.AddRange(p.Where(filter));
            }
            return cardList.ToArray();
        }
        public static int getTurnCount(this Player player)
        {
            return player.getProp<int>(PLAYER.TURNCOUNT);
        }
        #endregion

        #endregion

        #region 私有方法
        internal static Task initReplace(this Player player, CardEngine game, params Card[] cards)
        {
            return InitReplaceEventDefine.doEvent(game, player, cards);
        }
        #endregion

        #region 委托
        public delegate bool PoolRange(CardEngine game, CardDefine define);
        public delegate bool isInRange(CardEngine game, CardDefine define);
        #endregion

        #region 属性字段
        [TranslateMsg]
        const string NO_SPACE_FOR_TRAP = "你满头是包";
        [TranslateMsg]
        const string DUPLICATED_TRAP = "你已挂载了相同的陷阱";
        [TranslateMsg]
        const string ONLY_ONE_MISSION = "专注于你手头上的委托！";
        #endregion
    }
}