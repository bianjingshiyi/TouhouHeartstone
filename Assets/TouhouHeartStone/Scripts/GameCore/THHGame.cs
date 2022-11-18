using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone.Builtin;
using UnityEngine;

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
            return game.getRule().createPlayer(game, id, name, master.id, deck.Select(d => d.id).ToArray());
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
                throw new IndexOutOfRangeException(lastPlayer + "不在玩家行动队列中");
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

        public static THHCardDefine[] getTHHDefines(this CardEngine game)
        {
            return (THHCardDefine[])game.getDefines();
        }

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
        /// 死亡结算，清理战场上死亡的随从
        /// </summary>
        /// <returns></returns>
        public static Task updateDeath(this CardEngine game)
        {
            return game.getRule().updateDeath(game);
        }

        public static ITimer getTurnTimer(this CardEngine game)
        {
            return game.getRule().getTurnTimer(game);
        }

        internal static Task surrender(this CardEngine game, Player player)
        {
            game.logger.logTrace(player + "投降");
            return player.getMaster().die(game);
        }

        public async static Task leave(this CardEngine game, Player player)
        {
            await player.getMaster().die(game);
        }

        public static event Action<int, IResponse> onPlayerResponse;
        public static event Action onEnd;

        public static void logPlayerAction(this CardEngine game, int playerID, IResponse response)
        {
            onPlayerResponse?.Invoke(playerID, response);
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
        public static async Task<int> askSelect(this CardEngine game, Player player, int[] cards, int count = 3,
            string title = "选择一项")
        {
            if (cards.Count() < 1)
                return -1;
            if (count > 0)
                cards = cards.randomTake(game, count).ToArray();
            var eventArg = await game.triggers.doEvent(new THHPlayer.SelectEventArg()
            {
                player = player,
                options = cards
            }, async arg =>
            {
                player = arg.player;
                cards = arg.options;
                game.logger.logTrace(player + "要从卡牌或卡牌定义ID" + string.Join("，", cards) + "中选择一个");
                var res = await game.answers.ask(player.id, new DiscoverRequest(cards, title), float.MaxValue);
                int selectedID = cards.First(id => id == (res as DiscoverResponse).cardId);
                game.logger.logTrace(player + "选择" + selectedID);
                arg.selected = selectedID;
            });
            return eventArg.selected;
        }

        public static async Task<Card> askSelect(this CardEngine game, Player player, IEnumerable<Card> cards, int count = 3,
            string title = null)
        {
            var selectedId = await game.askSelect(player, cards.Select(c => c.id).ToArray(), count, title);
            Card selectedCard = cards.FirstOrDefault(c => c.id == selectedId);
            return selectedCard;
        }

        public static async Task<CardDefine> askSelect(this CardEngine game, Player player, IEnumerable<CardDefine> cards,
            int count = 3, string title = null)
        {
            var selectedId = await game.askSelect(player, cards.Select(c => c.id).ToArray(), count, title);
            CardDefine selectedCard = cards.FirstOrDefault(c => c.id == selectedId);
            return selectedCard;
        }
    }

    [Serializable]
    public class HearthstoneOption : GameOption
    {
        public static HearthstoneOption Default => new HearthstoneOption();

        // public int randomSeed = 0;
        public int[] sortedPlayers = null;
        public bool shuffle = true;
        public bool allowCustomCards = true;
        public float timeoutForInitReplace = 30;
        public float timeoutForTurn = 75;
        public int startGem = 0;

        public HearthstoneOption()
        {
        }

        public HearthstoneOption(HearthstoneOption copyTarget)
        {
            randomSeed = copyTarget.randomSeed;
            sortedPlayers = copyTarget.sortedPlayers;
            shuffle = copyTarget.shuffle;
            allowCustomCards = copyTarget.allowCustomCards;
            timeoutForInitReplace = copyTarget.timeoutForInitReplace;
            timeoutForTurn = copyTarget.timeoutForTurn;
        }
    }

    [Serializable]
    public class NotYourTurnException : Exception
    {
        public NotYourTurnException()
        {
        }

        public NotYourTurnException(TouhouCardEngine.Player player) : base("当前回合不是" + player + "的回合")
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

        public AlreadyPreparedException(TouhouCardEngine.Player player) : base(player + "已经准备过了")
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
        public override Task onGameStart(CardEngine game)
        {
            return game.triggers.doEvent(new InitEventArg(), async arg =>
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

                //设置生命值，洗牌，然后抽初始卡牌
                Player[] playersArray = getSortedPlayers(game);
                for (int i = 0; i < playersArray.Length; i++)
                {
                    Player player = playersArray[i];
                    Card master = player.getMaster();
                    await player.getMaster().setCurrentLife(game, 30);
                    master.InitConsumableKeywords(game);

                    if (player.getDeck().count < 1)
                        continue;
                    if (getOption(game).shuffle)
                        player.getDeck().shuffle(game);
                    int count = i == 0 ? 3 : 4;
                    var cards = player.getDeck().Where(c => c.hasKeyword(game, KEYWORD.MISSION))
                        .randomTake(game, count).ToList();
                    cards.AddRange(player.getDeck().Except(cards).Reverse()
                        .Take(count - cards.Count()));
                    cards.Reverse();
                    // var cards = getSortedPlayers(game)[i].getDeck().Skip(getSortedPlayers(game)[i].getDeck().count - count).Reverse()
                    //     .ToArray();
                    await player.getDeck().moveTo(game, cards, player.getInit());
                }

                game.logger.logTrace("游戏初始化，玩家行动顺序："
                                     + string.Join("、", playersArray.Select(p => p.ToString())) + "，"
                                     + "玩家英雄：" + string.Join("，",
                                         getSortedPlayers(game).Select(p => p.getMaster() + "(" + p.getSkill() + ")")) +
                                     "，"
                                     + "初始卡牌：" + string.Join("；",
                                         getSortedPlayers(game).Select(p =>
                                             string.Join("、", p.getInit().Select(c => c.ToString())))));
            });
        }
        public override async Task onPlayerInit(CardEngine game, Player player)
        {
            player.addPile(new Pile(game, PILE.MASTER, maxCount: 1));
            player.addPile(new Pile(game, PILE.SKILL, maxCount: 1));
            player.addPile(new Pile(game, PILE.DECK));
            player.addPile(new Pile(game, PILE.HAND, maxCount: 10));
            player.addPile(new Pile(game, PILE.INIT, maxCount: 4));
            player.addPile(new Pile(game, PILE.FIELD, maxCount: 7));
            player.addPile(new Pile(game, PILE.GRAVE));
            player.addPile(new Pile(game, PILE.WARP));
            player.addPile(new Pile(game, PILE.HEAD, maxCount: 5));
            player.addPile(new Pile(game, PILE.ITEM, maxCount: 1));
            int[] deck = player.getProp<int[]>(DEFINE.PLAYER_PROP_DECK);
            int master = deck[0];
            await player[PILE.MASTER].add(game, game.createCard(master));
            await player[PILE.SKILL].add(game, game.createCard(game.getDefine(master).getSkillId()));
            await player[PILE.DECK].add(game, deck.Select(d => game.createCard(d)));
        }
        #endregion
        public HearthStoneRule(IEnumerable<CardDefine> defines, IEnumerable<BuffDefine> buffs) : base(defines, buffs)
        {
        }

        public override Task onGameInit(CardEngine game, GameOption options, IRoomPlayer[] players)
        {
            game.triggers.onEventAfter += onEventAfterHandler;
            game.option = options;
            foreach (var playerInfo in players)
            {
                int[] deckIdArray = playerInfo.getProp<int[]>("deck");
                createPlayer(game, playerInfo.ID, playerInfo.Name, deckIdArray[0], deckIdArray.Skip(1).ToArray());
            }
            return Task.CompletedTask;
        }

        public override Task onGameRun(CardEngine game)
        {
            return gameflow(game);
        }

        public HearthstoneOption getOption(CardEngine game) => game.option as HearthstoneOption;

        public Player createPlayer(CardEngine game, int id, string name, int master, int[] deck)
        {
            //自定义英雄会被强制替换为灵梦，注释掉了
            //if (game.getDefine(master) is MasterCardDefine masterCardDefine)
            //{
            //}
            //else
            //{
            //    masterCardDefine = new Reimu();
            //    game.logger.logError("非法角色ID" + master + "被替换为灵梦");
            //}

            int luckyCoinId = new LuckyCoin().id;
            for (int i = 1; i < deck.Length; i++)
            {
                var define = game.getDefine(deck[i]);
                if (define == null)
                {
                    game.logger.logError("非法卡牌ID" + deck[i] + "被替换为幸运币");
                    deck[i] = luckyCoinId;
                }

                if (define is THHCardDefine d && d.isToken)
                {
                    game.logger.log("为什么你的牌库里会有衍生物？让我把它换成幸运币");
                    deck[i] = luckyCoinId;
                }
            }

            return createPlayer(game, id, name, game.getDefine(master), game.getDefines(deck));
        }

        public Player createPlayer(CardEngine game, int id, string name, CardDefine master,
            IEnumerable<CardDefine> deck)
        {
            if (game.players.Any(p => p.id == id))
                throw new ArgumentException("已经存在ID为" + id + "的玩家");
            Player player = new Player(id, name);
            player.addPile(new Pile(game, PILE.MASTER, 1));
            player.addPile(new Pile(game, PILE.SKILL, 1));
            player.addPile(new Pile(game, PILE.DECK));
            player.addPile(new Pile(game, PILE.HAND, maxCount: 10));
            player.addPile(new Pile(game, PILE.INIT, maxCount: 4));
            player.addPile(new Pile(game, PILE.FIELD, maxCount: 7));
            player.addPile(new Pile(game, PILE.GRAVE));
            player.addPile(new Pile(game, PILE.WARP));
            player.addPile(new Pile(game, PILE.HEAD, maxCount: 5));
            player.addPile(new Pile(game, PILE.ITEM, 1));
            player[PILE.MASTER].add(game, game.createCard(master));
            player[PILE.SKILL].add(game, game.createCard(game.getDefine(master.getSkillId())));
            player[PILE.DECK].add(game, deck.Select(d => game.createCard(d)));
            player.setIsPrepared(false);
            player.setGem(game, 0);
            player.setMaxGem(game, 0);
            player.setFatigue(0);
            game.addPlayer(player);
            return player;
        }

        public Player getPlayer(CardEngine game, int id)
        {
            return game.players.FirstOrDefault(p => p.id == id);
        }

        #region Gameflow

        public async Task gameflow(CardEngine game, Action onInited = null)
        {
            await init(game);
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

            await start(game);
            setCurrentPlayer(game, game.getSortedPlayers()[0]);
            for (int i = 0; i < 100; i++)
            {
                await turnStart(game, game.getCurrentPlayer());
                await turnLoop(game, game.getCurrentPlayer());
                await turnEnd(game, game.getCurrentPlayer());
                setCurrentPlayer(game, game.getPlayerForNextTurn(game.getCurrentPlayer()));
            }

            await gameEnd(game, new Player[0]);
        }

        internal async Task init(CardEngine game)
        {
            if (!game.isRunning)
                return;
            await game.triggers.doEvent(new HearthStoneRule.InitEventArg(), async arg =>
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
                Card[] masterCards = getSortedPlayers(game).Select(p => { return p.getMaster(); }).ToArray();
                foreach (Card card in masterCards)
                {
                    await card.setCurrentLife(game, card.getLife(game));
                }

                //洗牌，然后抽初始卡牌
                for (int i = 0; i < getSortedPlayers(game).Length; i++)
                {
                    if (getSortedPlayers(game)[i].getDeck().count < 1)
                        continue;
                    if (getOption(game).shuffle)
                        getSortedPlayers(game)[i].getDeck().shuffle(game);
                    int count = i == 0 ? 3 : 4;
                    var cards = getSortedPlayers(game)[i].getDeck().Where(c => c.hasKeyword(game, KEYWORD.MISSION))
                        .randomTake(game, count).ToList();
                    cards.AddRange(getSortedPlayers(game)[i].getDeck().Except(cards).Reverse()
                        .Take(count - cards.Count()));
                    cards.Reverse();
                    // var cards = getSortedPlayers(game)[i].getDeck().Skip(getSortedPlayers(game)[i].getDeck().count - count).Reverse()
                    //     .ToArray();
                    await getSortedPlayers(game)[i].getDeck().moveTo(game, cards, game.getSortedPlayers()[i].getInit());
                }

                game.logger.logTrace("游戏初始化，玩家行动顺序："
                                     + string.Join("、", game.getSortedPlayers().Select(p => p.ToString())) + "，"
                                     + "玩家英雄：" + string.Join("，",
                                         getSortedPlayers(game).Select(p => p.getMaster() + "(" + p.getSkill() + ")")) +
                                     "，"
                                     + "初始卡牌：" + string.Join("；",
                                         getSortedPlayers(game).Select(p =>
                                             string.Join("、", p.getInit().Select(c => c.ToString())))));
            });
        }

        public Player[] getSortedPlayers(CardEngine game) => game.getProp<Player[]>(GAME.SORTEDPLAYER);

        internal async Task start(CardEngine game)
        {
            if (!game.isRunning)
                return;
            await game.triggers.doEvent(new HearthStoneRule.StartEventArg(), async arg =>
            {
                game.logger.logTrace("游戏开始");
                foreach (Player player in getSortedPlayers(game))
                {
                    await player.setMaxGem(game, getOption(game).startGem);
                    await player.getInit().moveTo(game, player.getInit()[0, player.getInit().count - 1],
                        player.getHand(), 0);
                    if (player != getSortedPlayers(game)[0])
                    {
                        await player.getHand().add(game, game.createCard(game.getDefine<LuckyCoin>()));
                        game.logger.logTrace("由于后手行动" + player + "获得一张幸运币");
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
        public Player getCurrentPlayer(CardEngine game) => game.getProp<Player>(GAME.CURRENTPLAYER);

        async Task turnStart(CardEngine game, Player player)
        {
            if (!game.isRunning)
                return;
            await game.triggers.doEvent(new TurnStartEventArg() { player = player }, async arg =>
            {
                game.logger.logTrace(arg.player + "的回合开始");
                //玩家的最大能量加1但是不超过10，充满玩家的能量。
                await arg.player.setMaxGem(game, arg.player.getMaxGem() + 1);
                await arg.player.setGem(game, arg.player.getMaxGem());
                //抽一张牌
                await arg.player.draw(game);
                //重置技能
                await player.getSkill().setUsed(game, false);
                //使随从可以攻击
                foreach (Card card in player.getField())
                {
                    await card.setReady(game, true);
                    await card.setAttackTimes(game, 0);
                }

                //使自己可以攻击
                await player.getMaster().setAttackTimes(game, 0);
            });
            await game.updateDeath();
            //倒计时75秒
            setTurnTimer(game, game.time.startTimer(getOption(game).timeoutForTurn));
            getTurnTimer(game).onExpired += () => onTurnTimeout(game);
        }

        public void setTurnTimer(CardEngine game, ITimer value)
        {
            game.setProp(GAME.TURNTIMER, value);
        }

        public ITimer getTurnTimer(CardEngine game)
        {
            return game.getProp<ITimer>(GAME.TURNTIMER);
        }

        private void onTurnTimeout(CardEngine game)
        {
            game.logger.logTrace(game.getCurrentPlayer() + "回合超时");
            setTurnTimer(game, null);
            game.answers.cancel(game.answers.getRequests(game.getCurrentPlayer().id));
        }

        async Task turnLoop(CardEngine game, Player player)
        {
            if (!game.isRunning)
                return;
            for (int i = 0; i < 100; i++)
            {
                if (!game.isRunning)
                    return;
                if (getTurnTimer(game) == null) //超时，结束回合
                    return;
                IResponse response =
                    await game.answers.ask(player.id, new FreeActRequest(), getOption(game).timeoutForTurn * 2);
                switch (response)
                {
                    //自由输入
                    case UseResponse use:
                        Card card = game.getCard(use.cardId);
                        Card[] targets = game.getCards(use.targetsId);
                        if (!await player.tryUse(game, card, use.position, targets))
                            game.logger.log("Warning", "使用" + card + "失败");
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
        }

        public async Task turnEnd(CardEngine game, Player player)
        {
            if (!game.isRunning)
                return;
            await game.triggers.doEvent(new TurnEndEventArg() { player = player }, arg =>
            {
                game.logger.logTrace("Debug", getCurrentPlayer(game) + "回合结束");

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
                            chara.setFreeze(game, false);
                        }
                        else if ((chara.getMaxAttackTimes(game) - chara.getAttackTimes(game) >= 1) &&
                                chara.isMaster())
                        {
                            game.logger.logTrace("被冻结的英雄解除冰冻");
                            chara.setFreeze(game, false);
                        }
                    }
                }

                game.answers.cancel(game.answers.getRequests(player.id));
                return Task.CompletedTask;
            });
            game.time.cancel(getTurnTimer(game));
            setTurnTimer(game, null);
        }

        internal async Task gameEnd(CardEngine game, Player[] winners)
        {
            if (!game.isRunning)
                return;
            game.logger.logTrace("Debug", "游戏结束，" + string.Join("，", winners.Select(p => p.ToString())) + "获得游戏胜利");
            game.close();
            await game.triggers.doEvent(new HearthStoneRule.GameEndEventArg() { winners = winners });
        }

        /// <summary>
        /// 死亡结算，清理战场上死亡的随从
        /// </summary>
        /// <returns></returns>
        public Task updateDeath(CardEngine game)
        {
            List<Card> deathList = new List<Card>();
            foreach (Player player in game.players)
            {
                if (player.getMaster().isDead(game))
                {
                    deathList.Add(player.getMaster());
                }

                if (player.getItem() != null && player.getItem().isDead(game))
                {
                    deathList.Add(player.getItem());
                }

                foreach (Card card in player.getField())
                {
                    if (card.isDead(game))
                    {
                        deathList.Add(card);
                    }
                }
            }

            if (deathList.Count > 0)
            {
                return deathList.die(game);
            }
            else
                return Task.CompletedTask;
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
                        game.logger.log("Warning", "使用" + card + "失败");
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

        private void onEventAfterHandler(IEventArg arg)
        {
            if (arg is Card.AddModiEventArg addModi)
            {
                var game = addModi.game;
                // 添加护盾和隐匿关键字修改器后，使卡牌获得护盾和隐匿。
                if (addModi.modifier is KeywordsModifier keywordModi)
                {
                    if (keywordModi.value.Contains(KEYWORD.SHIELD))
                    {
                        addModi.card.setShield(game, true);
                    }
                    if (keywordModi.value.Contains(KEYWORD.STEALTH))
                    {
                        addModi.card.setStealth(game, true);
                    }
                }
            }
        }

        public class InitEventArg : EventArg
        {
        }

        public class StartEventArg : EventArg
        {
            public Dictionary<Player, StartInfo> startHandDic = new Dictionary<Player, StartInfo>();

            public class StartInfo
            {
                public int gem;
                public Card[] hands;
            }
            //public override EventVariableInfo[] getBeforeEventVarInfos()
            //{
            //    return new EventVariableInfo[]
            //    {
            //        new EventVariableInfo() { name = "手牌", type = typeof(Card[]) },
            //        new EventVariableInfo() { name = "初始灵力值", type = typeof(int) }
            //    };
            //}
            //public override EventVariableInfo[] getAfterEventVarInfos()
            //{
            //    return new EventVariableInfo[]
            //    {
            //        new EventVariableInfo() { name = "手牌", type = typeof(Card[]) },
            //        new EventVariableInfo() { name = "初始灵力值", type = typeof(int) }
            //    };
            //}
        }

        public class TurnStartEventArg : EventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
        }

        public class TurnEndEventArg : EventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = "玩家", type = typeof(Player) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = "玩家", type = typeof(Player) }
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
        }

        public class GameEndEventArg : EventArg
        {
            public Player[] winners;
        }

        public class InitReplaceEventArg : EventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = "玩家", type = typeof(Player) },
                    new EventVariableInfo() { name = "卡牌", type = typeof(Card[]) },
                    new EventVariableInfo() { name = "替换的卡牌", type = typeof(Card[]) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card[])},
                    new EventVariableInfo(){name = "替换的卡牌",type=typeof(Card[])}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
            public Card[] cards
            {
                get { return getVar<Card[]>("卡牌"); }
                set { setVar("卡牌", value); }
            }
            public Card[] replacedCards
            {
                get { return getVar<Card[]>("替换的卡牌"); }
                set { setVar("替换的卡牌", value); }
            }
        }

        public class SetMaxGemEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
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
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "值",type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "值",type=typeof(int)}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
            public int value
            {
                get { return getVar<int>("值"); }
                set { setVar("值", value); }
            }
        }

        public class BurnEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
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
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
            public Card card
            {
                get { return getVar<Card>("卡牌"); }
                set { setVar("卡牌", value); }
            }
        }

        /// <summary>
        /// 选择事件，一般指从几个选项中选择一项
        /// 卡面描述："选择一项"
        /// </summary>
        public class ChooseOneEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public IEnumerable<CardDefine> options;
            public CardDefine choose;
            public bool isShow = true;

            public ICard getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            public ICard[] getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是选项。")]
            const string TEMPLATE = "{0} 选择 {1}";

            [MukioI18n.TranslateMsg("{0}是玩家")] const string TEMPLATE_NotShow = "{0} 进行了选择";

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
                    choose.GetType().Name
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "Player",type=typeof(Player)},
                    new EventVariableInfo(){name = "Options;",type=typeof(IEnumerable<CardDefine>)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "Player",type=typeof(Player)},
                    new EventVariableInfo(){name = "Options;",type=typeof(IEnumerable<CardDefine>)},
                    new EventVariableInfo(){name = "Choose;",type=typeof(IEnumerable<CardDefine>)}
                };
            }
        }

        public class UseEventArg : EventArg, IDescribableEventArg
        {
            public UseEventArg(Player player, Card card, int position, Card[] targets, int cost)
            {
                setVar(VAR_PLAYER, player);
                setVar(VAR_CARD, card);
                setVar(VAR_POSITION, position);
                setVar(VAR_TARGETS, targets);
                setVar(VAR_COST, cost);
                setVar(VAR_TARGET_CARD, targets != null && targets.Length > 0 ? targets[0] : null);
            }
            public UseEventArg() : this(null, null, 0, null, 0)
            {
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = VAR_PLAYER, type=typeof(Player)},
                    new EventVariableInfo(){name = VAR_CARD, type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_POSITION, type=typeof(int)},
                    new EventVariableInfo(){name = VAR_COST, type=typeof(int)},
                    new EventVariableInfo() { name = VAR_TARGET_CARD, type = typeof(Card) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_POSITION,type=typeof(int)},
                    new EventVariableInfo(){name = VAR_COST,type=typeof(int)},
                    new EventVariableInfo() { name = VAR_TARGET_CARD, type = typeof(Card) }
                };
            }
            public Player player => getVar<Player>(VAR_PLAYER);
            public Card card => getVar<Card>(VAR_CARD);
            public int position => getVar<int>(VAR_POSITION);
            public int cost => getVar<int>(VAR_COST);
            public Card[] targets => getVar<Card[]>(VAR_TARGETS);

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                if (card.hasKeyword(game, KEYWORD.SECRET))
                    return viewer == player ? card : null;
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return targets;
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
                if (card.hasKeyword(game, KEYWORD.SECRET))
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
                    string.Join("，", targets?.Select(c => "{card:" + c.define.id + "}"))
                };
            }
            public const string VAR_PLAYER = "玩家";
            public const string VAR_CARD = "卡牌";
            public const string VAR_POSITION = "位置";
            public const string VAR_COST = "花费";
            public const string VAR_TARGETS = "目标";
            public const string VAR_TARGET_CARD = "目标卡牌";
        }

        public class EnterFieldEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

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
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)},
                    new EventVariableInfo(){name = "位置",type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)},
                    new EventVariableInfo(){name = "位置",type=typeof(int)}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
            public Pile from;
            public Card card
            {
                get { return getVar<Card>("卡牌"); }
                set { setVar("卡牌", value); }
            }
            public int position
            {
                get { return getVar<int>("位置"); }
                set { setVar("位置", value); }
            }
            public int attack;
            public int life;
        }

        public class RebornEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
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
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)},
                    new EventVariableInfo(){name = "位置",type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)},
                    new EventVariableInfo(){name = "位置",type=typeof(int)}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
            public int position
            {
                get { return getVar<int>("位置"); }
                set { setVar("位置", value); }
            }
            public Card card
            {
                get { return getVar<Card>("卡牌"); }
                set { setVar("卡牌", value); }
            }
        }

        public class RevealEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            public ICard[] getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

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
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = "玩家",type=typeof(Player)},
                    new EventVariableInfo(){name = "卡牌",type=typeof(Card)}
                };
            }
            public Player player
            {
                get { return getVar<Player>("玩家"); }
                set { setVar("玩家", value); }
            }
            /// <summary>
            /// 被揭示的牌
            /// </summary>
            public Card card
            {
                get { return getVar<Card>("卡牌"); }
                set { setVar("卡牌", value); }
            }
        }
    }
}