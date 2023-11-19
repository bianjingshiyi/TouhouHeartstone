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
        public static HearthStoneRule getRule(this CardEngine game) => game.rule as HearthStoneRule;

        #region Player

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="master"></param>
        /// <param name="deck">注意，牌库的第一张是牌库的底端</param>
        /// <returns></returns>
        public static Player createPlayer(this CardEngine game, int id, string name, CardDefine master,
            IEnumerable<CardDefine> deck)
        {
            return game.getRule().createPlayer(game, id, name, master, deck.ToArray());
        }

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
        public static Card createCard<T>(this CardEngine game) where T : CardDefine
        {
            return game.createCard(game.getDefine<T>());
        }

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

        public static Task<HearthStoneRule.GameEndEventArg> gameEnd(this CardEngine game, Player[] winners)
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
            DefineReference[] cardArray = cards.Select(c => c.define.getDefineRef()).ToArray();
            var res = await game.answers.ask(player.id, new DiscoverRequest(cardArray, title), float.MaxValue);
            Card selectedCard = cards.First(c => 
            {
                var defRef = (res as DiscoverResponse).cardDefRef;
                return c.define.cardPoolId == defRef.cardPoolId && c.define.id == defRef.defineId;
            });
            game.logger.logTrace($"{player}选择{selectedCard}");
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
            DefineReference[] cardArray = cards.Select(c => c.getDefineRef()).ToArray();
            var res = await game.answers.ask(player.id, new DiscoverRequest(cardArray, title), float.MaxValue);
            CardDefine selectedCard = cards.First(c =>
            {
                var defRef = (res as DiscoverResponse).cardDefRef;
                return c.cardPoolId == defRef.cardPoolId && c.id == defRef.defineId;
            });
            game.logger.logTrace($"{player}选择{selectedCard}");
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
        public static Task<HearthStoneRule.RevealEventArg> Reveal(this CardEngine game, Card card, bool shouldAnimate = true)
        {
            if (card == null)
                return Task.FromResult<HearthStoneRule.RevealEventArg>(null);
            return game.triggers.doEvent(new HearthStoneRule.RevealEventArg() { player = card?.getOwner(), card = card, shouldAnimate = shouldAnimate }, arg =>
            {
                var show = arg.card;
                game.logger.logTrace(show != null ? $"揭示了{show.getFormatString()}" : "什么也没揭示");
                return Task.CompletedTask;
            });
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
        public HearthStoneRule(Dictionary<long, Dictionary<int, CardDefine>> defines, Dictionary<long, Dictionary<int, BuffDefine>> buffs) : base(defines, buffs)
        {
        }
        #region 重写
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
        }
        public override Task onGameInit(CardEngine game, GameOption options, IRoomPlayer[] players)
        {
            game.option = options;
            game.logger.log($"游戏开始，本局游戏设置：{options.ToJson()}");
            foreach (var playerInfo in players)
            {
                Deck deck = playerInfo.getDeck();
                createPlayer(
                    game,
                    playerInfo.ID,
                    playerInfo.Name,
                    game.getDefine(deck.masterCardRef),
                    Array.ConvertAll(deck.toDefRefArray(false), r => game.getDefine(r.cardPoolId, r.defineId)).ToArray());
            }
            return init(game);
        }
        public override Task onGameStart(CardEngine game)
        {
            return start(game);
        }
        public override Task onGameRun(CardEngine game)
        {
            return gameflow(game);
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
        public Player createPlayer(CardEngine game, int id, string name, CardDefine master, CardDefine[] deck)
        {
            CardDefine luckyCoinDefine = game.getDefine(1, ClassicCardID.LuckyCoin);
            for (int i = 1; i < deck.Length; i++)
            {
                var define = deck[i];
                if (define == null)
                {
                    game.logger.logError($"非法卡牌ID{deck[i]}被替换为幸运币");
                    deck[i] = luckyCoinDefine;
                }

                if (define.isToken())
                {
                    game.logger.log("为什么你的牌库里会有衍生物？让我把它换成幸运币");
                    deck[i] = luckyCoinDefine;
                }
            }

            if (game.players.Any(p => p.id == id))
                throw new ArgumentException($"已经存在ID为{id}的玩家");
            Player player = new Player(id, name);

            game.initPlayer(player);

            player[PILE.MASTER].add(game, game.createCard(master)).Wait();
            DefineReference skillRef = master.getSkillRef();
            // TODO: 要么把CardEngine.GetDefine()改成返回null，要么加个CardEngine.TryGetDefine();
            var skillDef = game.getDefine(skillRef.cardPoolId, skillRef.defineId);
            if (skillDef != null)
            {
                player[PILE.SKILL].add(game, game.createCard(skillDef)).Wait();
            }
            player[PILE.DECK].add(game, Array.ConvertAll(deck, d => game.createCard(d)).Reverse()).Wait();

            game.addPlayer(player);
            return player;
        }
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
            await updateAura(game);

            bool anyCleared = false;
            const int MAX_UPDATE_TIMES = 30;
            List<Card> deathList = new List<Card>();
            int updateTimes = 0;
            do
            {
                var eventArg = new UpdateDeathEventArg();
                await game.triggers.doEvent(eventArg, func);
                updateTimes++;
            }
            // 受不了了，什么寄吧卡能让死亡结算运行30多次，跳了
            while (deathList.Count > 0 && game.isRunning && updateTimes < MAX_UPDATE_TIMES);

            // 如果有死亡的卡牌，再更新一次光环。
            if (anyCleared)
            {
                await updateAura(game);
            }
            // 死亡结算。
            async Task func(UpdateDeathEventArg arg)
            {
                deathList.Clear();
                foreach (Player player in game.players)
                {
                    deathList.AddRange(player.getField().Where(s => s != null && s.isDead(game)));

                    var item = player.getItem();
                    if (item != null && item.isDead(game))
                        deathList.Add(item);

                    var master = player.getMaster();
                    if (master != null && master.isDead(game))
                        deathList.Add(master);
                }

                if (deathList.Count <= 0 || !game.isRunning)
                {
                    return;
                }
                await deathList.destroy(game);
                anyCleared = true;
            }
        }
        #endregion

        private async Task updateAura(CardEngine game)
        {
            var cards = game.players.SelectMany(p => p.getPiles().SelectMany(pile => pile));
            foreach (var card in cards)
            {
                await card.updateAuras(game);
            }
        }
        #region Gameflow

        private async Task gameflow(CardEngine game, Action onInited = null)
        {
            Task<Dictionary<int, IResponse>> task = game.answers.askAll(
                game.getSortedPlayers().Select(p => p.id).ToArray(),
                new InitReplaceRequest()
                {
                }, getOption(game).timeoutForInitReplace);
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

            await game.startGame();
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

        private async Task init(CardEngine game)
        {
            await game.triggers.doEvent(new InitEventArg(), async arg =>
            {
                //决定玩家行动顺序
                if (game.getOption().sortedPlayers == null ||
                    game.getOption().sortedPlayers.Length != game.players.Length)
                {
                    if (game.getOption().sortedPlayers != null &&
                        game.getOption().sortedPlayers.Length != game.players.Length)
                        game.logger?.logWarn("游戏参数玩家行动顺序长度与实际数量不匹配");
                    List<Player> remainedList = new List<Player>(game.players);
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
                    game.setProp(GAME.SORTEDPLAYER,
                        game.getOption().sortedPlayers.Select(id => game.players.FirstOrDefault(p => p.id == id))
                            .ToArray());

                //创建主人公和技能卡
                var sortPlayers = getSortedPlayers(game);
                Card[] masterCards = sortPlayers.Select(p => { return p.getMaster(); }).ToArray();
                foreach (Card card in masterCards)
                {
                    await card.setCurrentLife(game, card.getLife(game));
                }

                //洗牌，然后抽初始卡牌
                for (int i = 0; i < sortPlayers.Length; i++)
                {
                    Player player = sortPlayers[i];
                    Pile deck = player.getDeck();
                    if (deck.count < 1)
                        continue;
                    if (getOption(game).shuffle)
                        deck.shuffle(game);
                    int count = i == 0 ? 3 : 4;
                    var initCards = deck.Where(c => c.hasKeyword(game, KEYWORD.MISSION))
                        .randomTake(game, count).ToList();
                    initCards.AddRange(deck.Except(initCards)
                        .Reverse()
                        .Take(count - initCards.Count()));
                    initCards.Reverse();
                    // var cards = deck.Skip(deck.count - count).Reverse()
                    //     .ToArray();
                    await deck.moveTo(game, initCards, player.getInit());
                }

                initRuleTriggers(game);

                var orderLog = string.Join("、", sortPlayers.Select(p => p.ToString()));
                var masterLog = string.Join("，", sortPlayers.Select(p => $"{p.getMaster()}({ p.getSkill()})"));
                var cardsLog = string.Join("；", sortPlayers.Select(p => string.Join("、", p.getInit().Select(c => c.ToString()))));
                game.logger.logTrace($"游戏初始化，玩家行动顺序：{orderLog}，玩家英雄：{masterLog}，初始卡牌：{cardsLog}");
            });
        }
        private async Task start(CardEngine game)
        {
            if (!game.isRunning)
                return;
            await game.triggers.doEvent(new HearthStoneRule.StartEventArg(), async arg =>
            {
                game.logger.logTrace("游戏开始");
                foreach (Player player in getSortedPlayers(game))
                {
                    await player.setMaxGem(game, getOption(game).startGem);
                    var initPile = player.getInit();
                    await initPile.moveTo(game, initPile, player.getHand(), 0);
                    if (player != getSortedPlayers(game)[0])
                    {
                        await player.getHand().add(game, game.createCard(game.getDefine(1, ClassicCardID.LuckyCoin)));
                        game.logger.logTrace($"由于后手行动{player}获得一张幸运币");
                    }

                    arg.startHandDic.Add(player, new HearthStoneRule.StartEventArg.StartInfo()
                    {
                        gem = player.getMaxGem(),
                        hands = player.getHand().ToArray()
                    });
                }
            });
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

            await game.triggers.doEvent(new TurnStartEventArg() { player = player }, async arg =>
            {
                game.logger.logTrace($"{arg.player}的回合开始");
                //玩家的最大能量加1但是不超过10，充满玩家的能量。
                await arg.player.setMaxGem(game, arg.player.getMaxGem() + 1);
                await arg.player.setGem(game, arg.player.getMaxGem());
                //抽一张牌
                await arg.player.draw(game);
                //重置技能
                await player.getSkill().setUsed(game, false);
            });
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
                        Task<THHCard.AttackEventArg> task = card.tryAttack(game, player, target);
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
            await game.triggers.doEvent(new TurnEndEventArg() { player = player }, async arg =>
            {
                game.logger.logTrace("Debug", $"{getCurrentPlayer(game)}回合结束");

                List<Card> AllCharactor = new List<Card>();
                AllCharactor.Add(player.getMaster());
                AllCharactor.AddRange(player.getField());
                foreach (Card chara in AllCharactor)
                {
                    if (chara.isFreeze(game))
                    {
                        //chara.setAttackTimes(chara.getAttackTimes(this) + 1);
                        if ((chara.getMaxAttackTimes(game) - chara.getAttackTimes(game) >= 1) &&
                            chara.isServant() && chara.isReady(game))
                        {
                            game.logger.logTrace("被冻结的随从解除冰冻");
                            await chara.setFreeze(game, false);
                        }
                        else if ((chara.getMaxAttackTimes(game) - chara.getAttackTimes(game) >= 1) &&
                                chara.isMaster())
                        {
                            game.logger.logTrace("被冻结的英雄解除冰冻");
                            await chara.setFreeze(game, false);
                        }
                    }
                }
                // 蓄力。
                foreach (Card hand in player.getHand())
                {
                    if (hand.canBeCharged(game) && player.getMaxGem() >= hand.getCost(game))
                    {
                        await hand.setChargeLevel(game, hand.getChargedLevel(game) + 1);
                    }
                }

                game.answers.cancel(game.answers.getRequests(player.id));
            });
            await game.updateGameState();
            game.time.cancel(getTurnTimer(game));
            setTurnTimer(game, null);
        }
        internal Task<GameEndEventArg> gameEnd(CardEngine game, Player[] winners)
        {
            if (!game.isRunning)
                return Task.FromResult<GameEndEventArg>(null);
            game.logger.logTrace("Debug", $"游戏结束，{string.Join("，", winners.Select(p => p.ToString()))}获得游戏胜利");
            game.close();
            return game.triggers.doEvent(new GameEndEventArg() { winners = winners });
        }
        public override async Task onPlayerCommand(CardEngine game, Player player, CardEngine.CommandEventArg command)
        {
            throw new NotImplementedException();

            IResponse response =
                await game.answers.ask(player.id, new FreeActRequest(), getOption(game).timeoutForTurn * 2);
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
                    Task<THHCard.AttackEventArg> task = card.tryAttack(game, player, target);
                    await task;
                    break;
                case TurnEndResponse _:
                    return;
                case SurrenderResponse _:
                    await game.surrender(player);
                    return;
            }
        }

        #endregion
        private void initRuleTriggers(CardEngine game)
        {
            foreach (var player in game.getSortedPlayers())
            {
                var propChangeTrigger = new Trigger<Card.PropChangeEventArg>(
                    condition: a => a.card == player.getItem() && a.propName == DEFINE.ATTACK,
                    action: async a => await updateMasterItemAttack(game, player));
                game.triggers.registerAfter(propChangeTrigger);

                var setDefineTrigger = new Trigger<Card.SetDefineEventArg>(
                    condition: a => a.card == player.getItem(),
                    action: async a => await updateMasterItemAttack(game, player));
                game.triggers.registerAfter(setDefineTrigger);

                var moveTrigger = new Trigger<THHPile.MoveCardEventArg>(
                    condition: a => a.from == player.getPile(PILE.ITEM) || a.to == player.getPile(PILE.ITEM),
                    action: async a => await updateMasterItemAttack(game, player));
                game.triggers.registerAfter(moveTrigger);
            }
        }
        private async Task updateMasterItemAttack(CardEngine game, Player player)
        {
            const string modifierPropName = "MASTER_ITEM_ATK_MODI";
            var master = player.getMaster();
            var modi = player.getProp<PropModifier>(modifierPropName);
            if (modi == null)
            {
                modi = new AttackModifier(0);
                player.setProp(game, modifierPropName, modi);
            }
            if (!master.getModifiers().Contains(modi))
            {
                await master.addModifier(game, modi);
            }
            var attack = 0;
            if (game.getCurrentPlayer() == player)
            {
                attack = player.getItem()?.getAttack(game) ?? 0;
            }
            await modi.setValue(game, master, attack);
        }

        public class InitEventArg : EventArg
        {
            public override void Record(IGame game, EventRecord record)
            {
            }
        }

        public class StartEventArg : EventArg
        {
            public Dictionary<Player, StartInfo> startHandDic = new Dictionary<Player, StartInfo>();

            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(VAR_START_HAND, startHandDic);
            }
            public class StartInfo
            {
                public int gem;
                public Card[] hands;
            };
            public const string VAR_START_HAND = "起始手牌";
        }

        public class TurnStartEventArg : EventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
        }

        public class TurnEndEventArg : EventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) }
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
        }

        public class GameEndEventArg : EventArg
        {
            public Player[] winners;
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(VAR_WINNERS, winners);
            }
            public const string VAR_WINNERS = "获胜玩家";
        }

        public class InitReplaceEventArg : EventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                    new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                    new EventVariableInfo() { name = VAR_REPLACED_CARDS, type = typeof(Card[]) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card[])},
                    new EventVariableInfo(){name = VAR_REPLACED_CARDS,type=typeof(Card[])}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardStates(EVENTVAR.CARD, cards);
                record.setCardStates(VAR_REPLACED_CARDS, replacedCards);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public Card[] cards
            {
                get { return getVar<Card[]>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            public Card[] replacedCards
            {
                get { return getVar<Card[]>(VAR_REPLACED_CARDS); }
                set { setVar(VAR_REPLACED_CARDS, value); }
            }
            public const string VAR_REPLACED_CARDS = "替换的卡牌";
        }

        public class SetMaxGemEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}:玩家，{1}值")]
            const string TEMPLATE = "{0}的最大灵力值变为{1}";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    value
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(EVENTVAR.VALUE, value);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public int value
            {
                get { return getVar<int>(EVENTVAR.VALUE); }
                set { setVar(EVENTVAR.VALUE, value); }
            }
        }

        public class BurnEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是手牌")]
            const string TEMPLATE = "{0}抽{1}，但是由于手牌已满它被送入墓地";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { player.name, card.getFormatString() };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }

        /// <summary>
        /// 选择卡牌定义事件
        /// </summary>
        public class SelectCardDefineEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (isShow)
                    return TEMPLATE;
                else
                    return TEMPLATE_NotShow;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    chosen.GetType().Name
                };
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = VAR_OPTIONS,type=typeof(IEnumerable<CardDefine>)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = VAR_OPTIONS,type=typeof(IEnumerable<CardDefine>)},
                    new EventVariableInfo(){name = EVENTVAR.CHOSEN_CARD,type=typeof(IEnumerable<CardDefine>)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(VAR_OPTIONS, options);
                record.setVar(EVENTVAR.CHOSEN_CARD, chosen);
                record.setVar(VAR_SEEN_CARD, isShow);
            }
            [MukioI18n.TranslateMsg("{0}是玩家，{1}是选项。")]
            const string TEMPLATE = "{0} 选择 {1}";
            [MukioI18n.TranslateMsg("{0}是玩家")] 
            const string TEMPLATE_NotShow = "{0} 进行了选择";
            public const string VAR_OPTIONS = "选项";
            public const string VAR_SEEN_CARD = "明牌";
            public Player player
            {
                get => getVar<Player>(EVENTVAR.PLAYER);
                set => setVar(EVENTVAR.PLAYER, value);
            }
            public CardDefine[] options
            {
                get => getVar<CardDefine[]>(VAR_OPTIONS);
                set => setVar(VAR_OPTIONS, value);
            }
            public CardDefine chosen
            {
                get => getVar<CardDefine>(EVENTVAR.CHOSEN_CARD);
                set => setVar(EVENTVAR.CHOSEN_CARD, value);
            }
            public bool isShow
            {
                get => getVar<bool>(VAR_SEEN_CARD);
                set => setVar(VAR_SEEN_CARD, value);
            }
        }

        public class UseEventArg : EventArg, IDescribableEventArg
        {
            public UseEventArg(Player player, Card card, int position, Card[] targets, int cost)
            {
                setVar(EVENTVAR.PLAYER, player);
                setVar(EVENTVAR.CARD, card);
                setVar(EVENTVAR.POSITION, position);
                setVar(EVENTVAR.TARGETS, targets);
                setVar(EVENTVAR.COST, cost);
                setVar(EVENTVAR.TARGET_CARD, targets != null && targets.Length > 0 ? targets[0] : null);
            }
            public UseEventArg() : this(null, null, 0, null, 0)
            {
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER, type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD, type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION, type=typeof(int)},
                    new EventVariableInfo(){name = EVENTVAR.COST, type=typeof(int)},
                    new EventVariableInfo() { name = EVENTVAR.TARGET_CARD, type = typeof(Card) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)},
                    new EventVariableInfo(){name = EVENTVAR.COST,type=typeof(int)},
                    new EventVariableInfo() { name = EVENTVAR.TARGET_CARD, type = typeof(Card) }
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(EVENTVAR.POSITION, position);
                record.setCardStates(EVENTVAR.TARGETS, targets);
                record.setVar(EVENTVAR.COST, cost);
                record.setCardState(EVENTVAR.TARGET_CARD, targets != null && targets.Length > 0 ? targets[0] : null);
            }
            public Player player => getVar<Player>(EVENTVAR.PLAYER);
            public Card card => getVar<Card>(EVENTVAR.CARD);
            public int position => getVar<int>(EVENTVAR.POSITION);
            public int cost => getVar<int>(EVENTVAR.COST);
            public Card[] targets => getVar<Card[]>(EVENTVAR.TARGETS);

            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                if (card.isTrap(game))
                    return viewer == player ? card : null;
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return targets.Select(t => (DescribableCard)t).ToArray();
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡。")]
            const string TEMPLATE_NO_TARGET = "{0} 使用 {1}";

            [MukioI18n.TranslateMsg("{0}是玩家，使用的是陷阱")]
            const string TEMPLATE_SECRET_OTHER = "{0} 使用了一张陷阱牌";

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是目标")]
            const string TEMPLATE_HAS_TARGET = "{0} 对 {2} 使用了 {1}";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (card.isTrap(game))
                {
                    return viewer == player ? TEMPLATE_NO_TARGET : TEMPLATE_SECRET_OTHER;
                }

                if (targets == null || targets.Length < 1)
                    return TEMPLATE_NO_TARGET;
                else
                    return TEMPLATE_HAS_TARGET;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    card?.getFormatString(),
                    string.Join("，", targets?.Select(c => c.getFormatString()))
                };
            }
        }

        [EventChildren(typeof(THHPile.MoveCardEventArg))]
        public class EnterFieldEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard() => card;

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是位置")]
            const string TEMPLATE = "{0} 将{1} 置入战场，位于 {2}";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    card.getFormatString(),
                    position
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
                record.setCardStates(EVENTVAR.FROM_PILE, from);
                record.setVar(EVENTVAR.POSITION, position);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public Pile from;
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            public int position
            {
                get { return getVar<int>(EVENTVAR.POSITION); }
                set { setVar(EVENTVAR.POSITION, value); }
            }
            public THHPile.MoveCardEventArg moveEvent;
            public int attack;
            public int life;
        }

        [EventChildren(typeof(THHPile.MoveCardEventArg))]
        public class EquipItemEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard()
            {
                return card;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE = "{0} 装备了 {1}";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    card.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(EVENTVAR.FROM_PILE, from);
                record.setCardState(EVENTVAR.CARD, card);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public Pile from;
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            public THHPile.MoveCardEventArg moveEvent;
        }
        public class RebornEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE = "{0} 复活了 {1}";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    card.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(EVENTVAR.POSITION, position);
                record.setCardState(EVENTVAR.CHOSEN_CARD, card);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public int position
            {
                get { return getVar<int>(EVENTVAR.POSITION); }
                set { setVar(EVENTVAR.POSITION, value); }
            }
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }

        public class RevealEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard() => card;

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE_VALID = "{0} 揭示了 {1}";

            [MukioI18n.TranslateMsg("{0}是玩家")] const string TEMPLATE_INVALID = "{0} 揭示了个寂寞";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return card == null ? TEMPLATE_INVALID : TEMPLATE_VALID;
            }

            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[]
                {
                    player.name,
                    card?.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(VAR_ANIMATE, shouldAnimate);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            /// <summary>
            /// 被揭示的牌
            /// </summary>
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            // 是否应该播放动画。
            public bool shouldAnimate;
            public const string VAR_ANIMATE = "播放动画";
        }
        public class UpdateDeathEventArg : EventArg
        {
            public override void Record(IGame game, EventRecord record)
            {
            }
        }
    }
}