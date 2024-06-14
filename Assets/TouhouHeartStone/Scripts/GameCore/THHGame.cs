using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class THHGame
    {
        public static HearthstoneOption getOption(this CardEngine game) => game.option as HearthstoneOption;
        public static HearthStoneRule getRule(this CardEngine game) => game.rules.OfType<HearthStoneRule>().FirstOrDefault();

        #region Player

        public static Player getPlayer(this CardEngine game, int id)
        {
            return getRule(game).getPlayer(game, id) as Player;
        }

        /// <summary>
        /// 获取下一个行动的玩家。
        /// </summary>
        /// <param name="lastPlayer"></param>
        /// <returns></returns>
        public static Player getPlayerForNextTurn(this CardEngine game, Player lastPlayer)
        {
            int index = Array.IndexOf(game.getSortedPlayers(), lastPlayer);
            if (index < 0)
                throw new IndexOutOfRangeException($"{lastPlayer}不在玩家行动队列中");
            index++;
            if (index >= game.getSortedPlayers().Length)
                index = 0;
            return game.getSortedPlayers()[index];
        }

        /// <summary>
        /// 获取玩家的对手。
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Player getOpponent(this CardEngine game, Player player)
        {
            return game.players.FirstOrDefault(p => p != player);
        }

        #endregion

        #region Card
        public static Card createCardByRandom(this CardEngine game, IEnumerable<CardDefine> defines)
        {
            return game.createCard(defines.random(game));
        }

        public static Card[] createCards(this CardEngine game, CardDefine[] defines)
        {
            return defines.Select(d => game.createCard(d)).ToArray();
        }

        public static Card[] findAllCardsInField(this CardEngine game, Func<Card, bool> filter)
        {
            List<Card> cardList = new List<Card>();
            foreach (Player player in game.players)
            {
                if (filter == null || filter.Invoke(player.getMaster()))
                    cardList.Add(player.getMaster());
                foreach (Card servant in player.getField())
                {
                    if (filter == null || filter.Invoke(servant))
                        cardList.Add(servant);
                }
            }

            return cardList.ToArray();
        }

        public static Card[] getAllCharacters(this CardEngine game)
        {
            List<Card> cardList = new List<Card>();
            foreach (var player in game.players)
            {
                cardList.Add(player.getMaster());
                cardList.AddRange(player.getField());
            }

            return cardList.ToArray();
        }

        public static Card[] getAllServants(this CardEngine game)
        {
            IEnumerable<Card> e = null;
            foreach (var player in game.players)
            {
                if (e == null)
                    e = player.getField();
                else
                    e = e.Concat(player.getField());
            }

            return e.ToArray();
        }

        public static Card[] getAllEnemies(this CardEngine game, Player player)
        {
            Player opponent = game.getOpponent(player);
            return opponent.getField().Concat(new Card[] { opponent.getMaster() }).ToArray();
        }

        public static Card[] getAllAllys(this CardEngine game, Player player)
        {
            return player.getField().Concat(new Card[] { player.getMaster() }).ToArray();
        }
        public static Task updateMasterItemAttack(this CardEngine game, Player player)
        {
            return game.getRule().updateMasterItemAttack(game, player);
        }
        #endregion

        #region Gameflow

        /// <summary>
        /// 运行游戏
        /// </summary>
        /// <returns>游戏运行的Task</returns>
        public static Task runGame(this CardEngine game)
        {
            return game.run();
        }

        public static Player[] getSortedPlayers(this CardEngine game) => game.getRule().getSortedPlayers(game);

        public static Player getCurrentPlayer(this CardEngine game) => game.getRule().getCurrentPlayer(game);

        /// <summary>
        /// 更新游戏状态，包括死亡结算，光环更新等。
        /// </summary>
        /// <returns></returns>
        public static Task updateGameState(this CardEngine game)
        {
            return game.getRule().updateGameState(game);
        }

        public static int getTurnCount(this CardEngine game)
        {
            return game.getRule().getTurnCount(game);
        }
        public static ITimer getTurnTimer(this CardEngine game)
        {
            return game.getRule().getTurnTimer(game);
        }

        internal static Task surrender(this CardEngine game, Player player)
        {
            game.logger.logTrace($"{player}投降");
            return player.getMaster().kill(game);
        }

        public async static Task leave(this CardEngine game, Player player)
        {
            await player.getMaster().kill(game);
        }

        public static event Action<int, IResponse> onPlayerResponse;

        public static void logPlayerAction(this CardEngine game, int playerID, IResponse response)
        {
            onPlayerResponse?.Invoke(playerID, response);
        }

        public static Task<EventArg> gameEnd(this CardEngine game, Player[] winners)
        {
            return game.getRule().gameEnd(game, winners);
        }
        #endregion
        /// <summary>
        /// 向玩家发出一个选择请求，玩家从提供的卡牌中选择一张，返回玩家选择的结果
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cards">卡牌的ID，可以是游戏内的卡牌ID，也可以是卡牌定义的ID</param>
        /// <param name="count">如果小于等于0，则全都可以挑</param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<Card> askSelect(this CardEngine game, Player player, IEnumerable<Card> cards, int count = 3,
            string title = null)
        {
            cards = cards.Where(c => c != null);
            if (cards.Count() < 1)
                return null;
            if (count > 0)
                cards = cards.randomTake(game, count).ToArray();
            game.logger.logTrace($"{player}要从卡牌{string.Join("，", cards)}中选择一个");
            Card selectedCard;
            if (game.getCurrentPlayer() == player)
            {
                int[] cardIdArray = cards.Select(c => c.id).ToArray();
                var res = await game.answers.ask(player.id, new DiscoverRequest(cardIdArray, title), float.MaxValue);
                selectedCard = cards.First(c =>
                {
                    var cardId = (res as DiscoverResponse).cardId;
                    return c.id == cardId;
                });
                game.logger.logTrace($"{player}选择{selectedCard}");
            }
            else
            {
                selectedCard = cards.random(game);
                game.logger.logTrace($"目前不是{player}的回合，{player}自动选择{selectedCard}");
            }
            return selectedCard;
        }
        /// <summary>
        /// 向玩家发出一个选择请求，卡牌定义中选择一张，返回玩家选择的结果
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cards">卡牌的ID，可以是游戏内的卡牌ID，也可以是卡牌定义的ID</param>
        /// <param name="count">如果小于等于0，则全都可以挑</param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<CardDefine> askSelect(this CardEngine game, Player player, IEnumerable<CardDefine> cards,
            int count = 3, string title = null)
        {
            cards = cards.Where(c => c != null);
            if (cards.Count() < 1)
                return null;
            if (count > 0)
                cards = cards.randomTake(game, count).ToArray();
            game.logger.logTrace($"{player}要从卡牌{string.Join("，", cards)}中选择一个");
            CardDefine selectedCard;
            if (game.getCurrentPlayer() == player)
            {
                DefineReference[] cardArray = cards.Select(c => c.getDefineRef()).ToArray();
                var res = await game.answers.ask(player.id, new DiscoverRequest(cardArray, title), float.MaxValue);
                selectedCard = cards.First(c =>
                {
                    var defRef = (res as DiscoverResponse).cardDefRef;
                    return c.cardPoolId == defRef.cardPoolId && c.id == defRef.defineId;
                });
                game.logger.logTrace($"{player}选择{selectedCard}");
            }
            else
            {
                selectedCard = cards.random(game);
                game.logger.logTrace($"目前不是{player}的回合，{player}自动选择{selectedCard}");
            }
            return selectedCard;
        }

        #region 揭示

        /// <summary>
        /// 揭示Reveal
        /// 向双方展示牌库里的一张牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards">被展示卡的来源</param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static async Task<Card> Reveal(this CardEngine game, Pile cards, Predicate<Card> filter = null, bool shouldAnimate = true)
        {
            Card show = null;

            var filtered = cards.Where(c => filter != null ? filter.Invoke(c) : true);
            if (filtered.Count() > 0)
            {
                show = filtered.random(game);
            }
            await game.Reveal(show, shouldAnimate);
            return show;
        }

        /// <summary>
        /// 揭示Reveal
        /// 向双方展示目标卡牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card">被展示的卡</param>
        /// <returns></returns>
        public static Task<EventArg> Reveal(this CardEngine game, Card card, bool shouldAnimate = true)
        {
            if (card == null)
                return Task.FromResult<EventArg>(null);
            return RevealEventDefine.doEvent(game, card.getOwner(), card, shouldAnimate);
        }

        public static Task<EventArg> revealCardDefine(this CardEngine game, CardDefine cardDefine, Player player, bool invisibleForSelf = false, bool invisibleForOppo = false)
        {
            if (cardDefine == null)
                return Task.FromResult<EventArg>(null);
            return RevealCardDefineEventDefine.doEvent(game, cardDefine, player, invisibleForSelf, invisibleForOppo);
        }

        #endregion
    }

    [Serializable]
    public class NotYourTurnException : Exception
    {
        public NotYourTurnException()
        {
        }

        public NotYourTurnException(TouhouCardEngine.Player player) : base($"当前回合不是{player}的回合")
        {
        }

        public NotYourTurnException(string message) : base(message)
        {
        }

        public NotYourTurnException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NotYourTurnException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class AlreadyPreparedException : Exception
    {
        public AlreadyPreparedException()
        {
        }

        public AlreadyPreparedException(TouhouCardEngine.Player player) : base($"{player}已经准备过了")
        {
        }

        public AlreadyPreparedException(string message) : base(message)
        {
        }

        public AlreadyPreparedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AlreadyPreparedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class UnknowKeywordException : Exception
    {
        public UnknowKeywordException()
        {
        }

        public UnknowKeywordException(string message) : base(message)
        {
        }

        public UnknowKeywordException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnknowKeywordException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }

    public partial class HearthStoneRule : Rule
    {
        #region 公有方法
        public HearthStoneRule() : base()
        {
        }
        #region 重写
        public override Task onGameInit(CardEngine game, GameOption options)
        {
            game.logger.log($"游戏开始，本局游戏设置：{options.ToJson()}");
            return InitEventDefine.doEvent(game);
        }
        public override async Task onPlayerInit(CardEngine game, Player player)
        {
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
            var master = startDeck[0];
            var deckCardDefines = startDeck.Skip(1).ToArray();
            CardDefine luckyCoinDefine = game.getDefine(1, ClassicCardID.LuckyCoin);
            for (int i = 1; i < deckCardDefines.Length; i++)
            {
                var define = deckCardDefines[i];
                if (define == null)
                {
                    game.logger.logError($"非法卡牌ID{deckCardDefines[i]}被替换为幸运币");
                    deckCardDefines[i] = luckyCoinDefine;
                }

                if (define.isToken())
                {
                    game.logger.log("为什么你的牌库里会有衍生物？让我把它换成幸运币");
                    deckCardDefines[i] = luckyCoinDefine;
                }
            }

            DefineReference skillRef = master.getSkillRef();
            var skillDef = game.getDefine(skillRef.cardPoolId, skillRef.defineId);
            var masterCard = game.createCard(master);
            await Pile.moveCard(game, masterCard, null, player[PILE.MASTER], 0);
            await masterCard.resetBattlefieldProps(game);

            var skillCard = game.createCard(skillDef);
            await Pile.moveCard(game, skillCard, null, player[PILE.SKILL], 0);

            var deckCards = Array.ConvertAll(deckCardDefines, d => game.createCard(d)).Reverse();
            if (game.getOption().shuffle)
                deckCards = deckCards.shuffle(game);
            var deck = player[PILE.DECK];
            foreach (var deckCard in deckCards)
            {
                await Pile.moveCard(game, deckCard, null, deck, deck.count);
            }
        }
        public override Task onPlayerExit(CardEngine game, Player player)
        {
            game.logger.logWarn($"玩家{player}已退出游戏！");
            return Task.CompletedTask;
        }
        public override async Task onGameRun(CardEngine game)
        {
            await playerReady(game);
            await gameflow(game);
        }
        public override Task onGameClose(CardEngine game)
        {
            game.isRunning = false;
            if (getTurnTimer(game) != null)
            {
                game.time.cancel(getTurnTimer(game));
                setTurnTimer(game, null);
            }

            game.answers.cancelAll();
            // THHGame.onEnd?.Invoke();
            return Task.CompletedTask;
        }
        #endregion

        public HearthstoneOption getOption(CardEngine game) => game.option as HearthstoneOption;
        public Player getPlayer(CardEngine game, int id)
        {
            return game.players.FirstOrDefault(p => p.id == id);
        }
        public Player[] getSortedPlayers(CardEngine game) => game.getProp<Player[]>(GAME.SORTEDPLAYER);
        public Player getCurrentPlayer(CardEngine game) => game.getProp<Player>(GAME.CURRENTPLAYER);
        public void setTurnTimer(CardEngine game, ITimer value)
        {
            game.setProp(GAME.TURNTIMER, value);
        }
        public int getTurnCount(CardEngine game)
        {
            return game.getProp<int>(GAME.TURNCOUNT);
        }
        public ITimer getTurnTimer(CardEngine game)
        {
            return game.getProp<ITimer>(GAME.TURNTIMER);
        }
        /// <summary>
        /// 更新游戏状态，包括死亡结算，光环更新等。
        /// </summary>
        /// <returns></returns>
        public async Task updateGameState(CardEngine game)
        {
            // 更新光环。
            await updateCardStates(game);

            bool clearedThisTime = false;
            bool anyCleared = false;
            const int MAX_UPDATE_TIMES = 30;
            int updateTimes = 0;
            do
            {
                clearedThisTime = false;
                var eventArg = await UpdateDeathEventDefine.doEvent(game);
                if (eventArg.getVar<Card[]>(UpdateDeathEventDefine.VAR_CLEARED_CARDS).Length > 0)
                {
                    clearedThisTime = true;
                    anyCleared = true;
                }
                updateTimes++;
            }
            // 受不了了，什么寄吧卡能让死亡结算运行30多次，跳了
            while (clearedThisTime && game.isRunning && updateTimes < MAX_UPDATE_TIMES);

            // 如果有死亡的卡牌，再更新一次光环。
            if (anyCleared)
            {
                await updateCardStates(game);
            }
        }
        public async Task updateMasterItemAttack(CardEngine game, Player player)
        {
            const string buffPlayerPropName = "MASTER_ITEM_ATK_MODI";
            var master = player.getMaster();
            var buff = player.getProp<Buff>(buffPlayerPropName);
            if (buff == null)
            {
                var buffDefine = game.getBuffDefine(MasterItemAttackBuffDefine.buffDefineRef);
                buff = new Buff(buffDefine);
                await master.addBuff(game, buff);
                player.setProp(game, buffPlayerPropName, buff);
            }
            var attack = 0;
            if (game.getCurrentPlayer() == player)
            {
                attack = player.getItem()?.getAttack(game) ?? 0;
            }
            await buff.setProp(game, MasterItemAttackBuffDefine.buffPropName, attack);
        }
        #endregion

        private async Task updateCardStates(CardEngine game)
        {
            var cards = game.players.SelectMany(p => p.getPiles().SelectMany(pile => pile));
            foreach (var card in cards)
            {
                await card.updateAuras(game);
                await card.regainPernamentConsumableKeywords(game);
            }
        }
        #region Gameflow
        private async Task playerReady(CardEngine game)
        {
            var gamePlayers = game.players;
            //决定玩家行动顺序
            var optionPlayers = game.getOption().sortedPlayers;
            if (optionPlayers == null || optionPlayers.Length != gamePlayers.Length)
            {
                if (optionPlayers != null && optionPlayers.Length != gamePlayers.Length)
                    game.logger?.logWarn("游戏参数玩家行动顺序长度与实际数量不匹配");
                List<Player> remainedList = new List<Player>(gamePlayers);
                Player[] sortedPlayers = new Player[remainedList.Count];
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    int index = game.randomInt(0, remainedList.Count - 1);
                    sortedPlayers[i] = remainedList[index];
                    remainedList.RemoveAt(index);
                }
                game.setProp(GAME.SORTEDPLAYER, sortedPlayers);
            }
            else
            {
                game.setProp(GAME.SORTEDPLAYER, optionPlayers.Select(id => gamePlayers.FirstOrDefault(p => p.id == id)).ToArray());
            }

            // 抽初始卡牌
            var sortPlayers = game.getSortedPlayers();
            for (int i = 0; i < sortPlayers.Length; i++)
            {
                Player player = sortPlayers[i];
                Pile deck = player.getDeck();
                if (deck.count < 1)
                    continue;
                int count = i == 0 ? 3 : 4;
                var initCards = deck.Where(c => c.hasKeyword(game, KEYWORD.MISSION))
                    .randomTake(game, count).ToList();
                initCards.AddRange(deck.Except(initCards)
                    .Reverse()
                    .Take(count - initCards.Count()));
                initCards.Reverse();
                await deck.moveTo(game, initCards, player.getInit());
                await game.updateMasterItemAttack(player);
            }


            var orderLog = string.Join("、", sortPlayers.Select(p => p.ToString()));
            var masterLog = string.Join("，", sortPlayers.Select(p => $"{p.getMaster()}({p.getSkill()})"));
            var cardsLog = string.Join("；", sortPlayers.Select(p => string.Join("、", p.getInit().Select(c => c.ToString()))));
            game.logger.logTrace($"游戏初始化，玩家行动顺序：{orderLog}，玩家英雄：{masterLog}，初始卡牌：{cardsLog}");
        }
        private async Task gameflow(CardEngine game, Action onInited = null)
        {
            Task<Dictionary<int, IResponse>> task = game.answers.askAll(
                game.getSortedPlayers().Select(p => p.id).ToArray(),
                new InitReplaceRequest(), getOption(game).timeoutForInitReplace);
            onInited?.Invoke();
            Dictionary<int, IResponse> initReplaceResponses = await task;
            if (!game.isRunning)
                return;
            foreach (var result in initReplaceResponses)
            {
                Player player = game.getPlayer(result.Key);
                if (result.Value is InitReplaceResponse initReplace)
                    await player.initReplace(game, game.getCards(initReplace.cardsId));
                else if (result.Value is SurrenderResponse surrender)
                    await game.surrender(player);
            }

            await start(game);
            setCurrentPlayer(game, game.getSortedPlayers()[0]);
            for (int i = 0; i < 100; i++)
            {
                var turnCount = game.getTurnCount();
                game.setProp(GAME.TURNCOUNT, turnCount + 1);

                var curPlayer = getCurrentPlayer(game);
                var playerTurnCount = curPlayer.getTurnCount();
                curPlayer.setProp(game, PLAYER.TURNCOUNT, playerTurnCount + 1);

                await updateSecrets(game);
                await turnStart(game, game.getCurrentPlayer());
                await turnLoop(game, game.getCurrentPlayer());
                await turnEnd(game, game.getCurrentPlayer());
                setCurrentPlayer(game, game.getPlayerForNextTurn(game.getCurrentPlayer()));
            }

            await gameEnd(game, new Player[0]);
        }
        private Task start(CardEngine game)
        {
            if (!game.isRunning)
                return Task.CompletedTask;
            return StartEventDefine.doEvent(game);
        }

        private void setCurrentPlayer(CardEngine game, Player value) => game.setProp(GAME.CURRENTPLAYER, value);

        private async Task updateSecrets(CardEngine game)
        {
            var thisTurnPlayer = game.getCurrentPlayer();
            var lastTurnPlayer = game.getOpponent(thisTurnPlayer);
            await lastTurnPlayer.updateSecrets(game);
            await thisTurnPlayer.updateSecrets(game);
        }
        private async Task turnStart(CardEngine game, Player player)
        {
            if (!game.isRunning)
                return;

            // 更新攻击次数，放在回合开始事件前面，主要为了防止回合开始时召唤出来的仆从可以立即攻击。
            // 同时要更新所有角色的攻击次数，防止转移控制权的先攻仆从无法攻击。
            foreach (Player p in game.getSortedPlayers())
            {
                await updateMasterItemAttack(game, p);
                //使随从可以攻击
                foreach (var card in p.getField())
                {
                    await card.setReady(game, true);
                    await card.setAttackTimes(game, 0);
                }
                //使自己可以攻击
                await p.getMaster().setAttackTimes(game, 0);
            }

            await TurnStartEventDefine.doEvent(game, player);
            await game.updateGameState();
            //倒计时75秒
            setTurnTimer(game, game.time.startTimer(getOption(game).timeoutForTurn));
            getTurnTimer(game).onExpired += () => onTurnTimeout(game);
        }

        private void onTurnTimeout(CardEngine game)
        {
            game.logger.logTrace($"{game.getCurrentPlayer()}回合超时");
            setTurnTimer(game, null);
            game.answers.cancel(game.answers.getRequests(game.getCurrentPlayer().id));
        }

        private async Task turnLoop(CardEngine game, Player player)
        {
            if (!game.isRunning)
                return;
            Task<IResponse> surrenderResponseTask =
                game.answers.askAny(game.players.Select(p => p.id).ToArray(), new SurrenderRequest(), getOption(game).timeoutForTurn * 2, null);
            for (int i = 0; i < 100; i++)
            {
                if (!game.isRunning)
                    return;
                if (getTurnTimer(game) == null) //超时，结束回合
                    return;
                Task<IResponse> responseTask =
                    game.answers.ask(player.id, new FreeActRequest(), getOption(game).timeoutForTurn * 2);

                var finishedTask = await Task.WhenAny(surrenderResponseTask, responseTask);
                var response = await finishedTask;

                switch (response)
                {
                    //自由输入
                    case UseResponse use:
                        Card card = game.getCard(use.cardId);
                        Card[] targets = game.getCards(use.targetsId);
                        if (!await player.tryUse(game, card, use.position, targets))
                            game.logger.log("Warning", $"使用{card}失败");
                        break;
                    case AttackResponse attack:
                        card = game.getCard(attack.cardId);
                        Card target = game.getCard(attack.targetId);
                        Task<EventArg> task = card.tryAttack(game, target);
                        await task;
                        break;
                    case TurnEndResponse _:
                        return;
                    case SurrenderResponse surrender:
                        await game.surrender(game.getPlayer(surrender.playerId));
                        return;
                }
                await game.updateGameState();
            }
        }

        public async Task turnEnd(CardEngine game, Player player)
        {
            if (!game.isRunning)
                return;
            await TurnEndEventDefine.doEvent(game, player);
            await game.updateGameState();
            game.time.cancel(getTurnTimer(game));
            setTurnTimer(game, null);
        }
        internal Task<EventArg> gameEnd(CardEngine game, Player[] winners)
        {
            if (!game.isRunning)
                return Task.FromResult<EventArg>(null);
            game.logger.logTrace("Debug", $"游戏结束，{string.Join("，", winners.Select(p => p.ToString()))}获得游戏胜利");
            game.close();
            return GameEndEventDefine.doEvent(game, winners);
        }
        #endregion

    }
}