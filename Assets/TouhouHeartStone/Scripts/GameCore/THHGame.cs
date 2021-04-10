using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone.Builtin;

namespace TouhouHeartstone
{
    public class THHGame : CardEngine, IDisposable
    {
        public GameOption option { get; }

        public new TriggerManager triggers
        {
            get { return base.triggers as TriggerManager; }
            set { base.triggers = value; }
        }

        public new AnswerManager answers
        {
            get { return base.answers as AnswerManager; }
            set { base.answers = value; }
        }

        public new TimeManager time
        {
            get { return base.time as TimeManager; }
            set { base.time = value; }
        }

        public THHGame(params CardDefine[] defines) : base(GameOption.Default.randomSeed, defines)
        {
            option = GameOption.Default;
        }

        public THHGame(GameOption option, params CardDefine[] defines) : base(option.randomSeed, defines)
        {
            this.option = option;
        }

        #region Player

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="master"></param>
        /// <param name="deck">注意，牌库的第一张是牌库的底端</param>
        /// <returns></returns>
        public THHPlayer createPlayer(int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck)
        {
            if (players.Any(p => p.id == id))
                throw new ArgumentException("已经存在ID为" + id + "的玩家");
            THHPlayer player = new THHPlayer(this, id, name);
            player.addPile(new Pile(this, PILE.MASTER, 1));
            player.addPile(new Pile(this, PILE.SKILL, 1));
            player.addPile(new Pile(this, PILE.DECK));
            player.addPile(new Pile(this, PILE.HAND, maxCount: 10));
            player.addPile(new Pile(this, PILE.INIT, maxCount: 4));
            player.addPile(new Pile(this, PILE.FIELD, maxCount: 7));
            player.addPile(new Pile(this, PILE.GRAVE));
            player.addPile(new Pile(this, PILE.WARP));
            player.addPile(new Pile(this, PILE.HEAD, maxCount: 5));
            player.addPile(new Pile(this, PILE.ITEM, 1));
            player[PILE.MASTER].add(this, this.createCard(master));
            player[PILE.SKILL].add(this, this.createCard(this.getDefine(master.skillID)));
            player[PILE.DECK].add(this, deck.Select(d => this.createCard(d)));
            player.setIsPrepared(false);
            player.setGem(this, 0);
            player.setMaxGem(this, 0);
            player.setFatigue(0);
            addPlayer(player);
            return player;
        }

        public THHPlayer[] players
        {
            get { return getPlayers().Cast<THHPlayer>().ToArray(); }
        }

        public THHPlayer getPlayer(int id)
        {
            return players.FirstOrDefault(p => p.id == id);
        }

        /// <summary>
        /// 获取下一个行动的玩家。
        /// </summary>
        /// <param name="lastPlayer"></param>
        /// <returns></returns>
        public THHPlayer getPlayerForNextTurn(THHPlayer lastPlayer)
        {
            int index = Array.IndexOf(sortedPlayers, lastPlayer);
            if (index < 0)
                throw new IndexOutOfRangeException(lastPlayer + "不在玩家行动队列中");
            index++;
            if (index >= sortedPlayers.Length)
                index = 0;
            return sortedPlayers[index];
        }

        /// <summary>
        /// 获取玩家的对手。
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public THHPlayer getOpponent(Player player)
        {
            return players.FirstOrDefault(p => p != player);
        }

        public THHPlayer[] getAllPlayer()
        {
            return players;
        }

        #endregion

        #region Card

        public Card createCard(CardDefine define)
        {
            Card card = this.issueCard(define);
            if (define is MasterCardDefine master)
            {
                card.setLife(this, master.life);
            }
            else if (define is ServantCardDefine servant)
            {
                card.setCost(this, servant.cost);
                card.setAttack(this, servant.attack);
                card.setLife(this, servant.life);
                card.setSpellDamage(this, servant.spellDamage);

                foreach (string keyword in servant.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    card.setProp(this, keyword, true);
                }

                card.setProp(this, PROPERTY.KEYWORDS, servant.keywords);
                card.setProp(this, nameof(ServantCardDefine.tags), servant.tags);
            }
            else if (define is SpellCardDefine spell)
            {
                card.setCost(this, spell.cost);
                foreach (var keyword in spell.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    card.setProp(this, keyword, true);
                }

                card.setProp(this, PROPERTY.KEYWORDS, spell.keywords);
                card.setProp(this, nameof(SpellCardDefine.tags), spell.tags);
            }
            else if (define is SkillCardDefine skill)
            {
                card.setCost(this, skill.cost);
            }
            else if (define is ItemCardDefine item)
            {
                card.setCost(this, item.cost);
                card.setAttack(this, item.attack);
                card.setLife(this, item.life);
            }

            return card;
        }

        public Card createCard<T>() where T : CardDefine
        {
            return createCard(getDefine<T>());
        }

        public Card createCardByRandom(IEnumerable<CardDefine> defines)
        {
            return createCard(defines.random(this));
        }

        public Card[] createCards(CardDefine[] defines)
        {
            return defines.Select(d => createCard(d)).ToArray();
        }

        public Card[] findAllCardsInField(Func<Card, bool> filter)
        {
            List<Card> cardList = new List<Card>();
            foreach (THHPlayer player in players)
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

        public Card[] getAllCharacters()
        {
            List<Card> cardList = new List<Card>();
            foreach (var player in players)
            {
                cardList.Add(player.getMaster());
                cardList.AddRange(player.getField());
            }

            return cardList.ToArray();
        }

        public Card[] getAllServants()
        {
            IEnumerable<Card> e = null;
            foreach (var player in players)
            {
                if (e == null)
                    e = player.getField();
                else
                    e = e.Concat(player.getField());
            }

            return e.ToArray();
        }

        public Card[] getAllEnemies(THHPlayer player)
        {
            THHPlayer opponent = getOpponent(player);
            return opponent.getField().Concat(new Card[] {opponent.getMaster()}).ToArray();
        }

        public Card[] getAllAllys(THHPlayer player)
        {
            return player.getField().Concat(new Card[] {player.getMaster()}).ToArray();
        }



        #endregion

        #region Gameflow

        public bool isRunning { get; private set; } = false;

        /// <summary>
        /// 运行游戏
        /// </summary>
        /// <returns>游戏运行的Task</returns>
        public Task run(Action onInited = null)
        {
            isRunning = true;
            return gameflow(onInited);
        }

        private async Task gameflow(Action onInited = null)
        {
            await init();
            Task<Dictionary<int, IResponse>> task = answers.askAll(sortedPlayers.Select(p => p.id).ToArray(),
                new InitReplaceRequest()
                {
                }, option.timeoutForInitReplace);
            onInited?.Invoke();
            Dictionary<int, IResponse> initReplaceResponses = await task;
            if (!isRunning)
                return;
            foreach (var result in initReplaceResponses)
            {
                THHPlayer player = getPlayer(result.Key);
                if (result.Value is InitReplaceResponse initReplace)
                    await player.initReplace(this, getCards(initReplace.cardsId));
                else if (result.Value is SurrenderResponse surrender)
                    await this.surrender(player);
            }

            await start();
            currentPlayer = sortedPlayers[0];
            for (int i = 0; i < 100; i++)
            {
                await turnStart(currentPlayer);
                await turnLoop(currentPlayer);
                await turnEnd(currentPlayer);
                currentPlayer = getPlayerForNextTurn(currentPlayer);
            }

            await gameEnd(new THHPlayer[0]);
        }

        internal async Task init()
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new InitEventArg(), async arg =>
            {
                //决定玩家行动顺序
                if (option.sortedPlayers == null || option.sortedPlayers.Length != players.Length)
                {
                    if (option.sortedPlayers != null && option.sortedPlayers.Length != players.Length)
                        logger?.log("Warning", "游戏参数玩家行动顺序长度与实际数量不匹配");
                    List<THHPlayer> remainedList = new List<THHPlayer>(players);
                    THHPlayer[] sortedPlayers = new THHPlayer[remainedList.Count];
                    for (int i = 0; i < sortedPlayers.Length; i++)
                    {
                        int index = randomInt(0, remainedList.Count - 1);
                        sortedPlayers[i] = remainedList[index];
                        remainedList.RemoveAt(index);
                    }

                    this.sortedPlayers = sortedPlayers;
                }
                else
                    this.sortedPlayers = option.sortedPlayers.Select(id => players.FirstOrDefault(p => p.id == id))
                        .ToArray();

                //创建主人公和技能卡
                Card[] masterCards = sortedPlayers.Select(p => { return p.getMaster(); }).ToArray();
                foreach (Card card in masterCards)
                {
                    await card.setCurrentLife(this, 30);
                }

                //洗牌，然后抽初始卡牌
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    if (sortedPlayers[i].getDeck().count < 1)
                        continue;
                    if (option.shuffle)
                        sortedPlayers[i].getDeck().shuffle(this);
                    int count = i == 0 ? 3 : 4;
                    var cards = sortedPlayers[i].getDeck().Skip(sortedPlayers[i].getDeck().count - count).Reverse()
                        .ToArray();
                    await sortedPlayers[i].getDeck().moveTo(this, cards, sortedPlayers[i].getInit());
                }

                logger.log("Debug", "游戏初始化，玩家行动顺序："
                                    + string.Join("、", sortedPlayers.Select(p => p.ToString())) + "，"
                                    + "玩家英雄：" + string.Join("，",
                                        sortedPlayers.Select(p => p.getMaster() + "(" + p.getSkill() + ")")) + "，"
                                    + "初始卡牌：" + string.Join("；",
                                        sortedPlayers.Select(p =>
                                            string.Join("、", p.getInit().Select(c => c.ToString())))));
            });
        }

        public class InitEventArg : EventArg
        {
        }

        public THHPlayer[] sortedPlayers { get; private set; }

        internal async Task start()
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new StartEventArg(), async arg =>
            {
                logger.log("Debug", "游戏开始");
                foreach (THHPlayer player in sortedPlayers)
                {
                    await player.setMaxGem(this, option.startGem);
                    await player.getInit().moveTo(this, player.getInit()[0, player.getInit().count - 1],
                        player.getHand(), 0);
                    if (player != sortedPlayers[0])
                    {
                        await player.getHand().add(this, createCard(getDefine<LuckyCoin>()));
                        logger.log("由于后手行动" + player + "获得一张幸运币");
                    }

                    arg.startHandDic.Add(player, new StartEventArg.StartInfo()
                    {
                        gem = player.getMaxGem(),
                        hands = player.getHand().ToArray()
                    });
                }
            });
        }

        public class StartEventArg : EventArg
        {
            public Dictionary<THHPlayer, StartInfo> startHandDic = new Dictionary<THHPlayer, StartInfo>();

            public class StartInfo
            {
                public int gem;
                public Card[] hands;
            }
        }

        public THHPlayer currentPlayer { get; private set; }

        async Task turnStart(THHPlayer player)
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new TurnStartEventArg() {player = player}, async arg =>
            {
                logger.log("Debug", arg.player + "的回合开始");
                //玩家的最大能量加1但是不超过10，充满玩家的能量。
                await arg.player.setMaxGem(this, arg.player.getMaxGem() + 1);
                await arg.player.setGem(this, arg.player.getMaxGem());
                //抽一张牌
                await arg.player.draw(this);
                //重置技能
                await player.getSkill().setUsed(this, false);
                //使随从可以攻击
                foreach (Card card in player.getField())
                {
                    await card.setReady(this, true);
                    await card.setAttackTimes(this, 0);
                }

                //使自己可以攻击
                await player.getMaster().setAttackTimes(this, 0);
            });
            //倒计时75秒
            turnTimer = time.startTimer(option.timeoutForTurn);
            turnTimer.onExpired += onTurnTimeout;
        }

        public ITimer turnTimer { get; set; } = null;

        private void onTurnTimeout()
        {
            logger.log("Debug", currentPlayer + "回合超时");
            turnTimer = null;
            answers.cancel(answers.getRequests(currentPlayer.id));
        }

        public class TurnStartEventArg : EventArg
        {
            public THHPlayer player;
        }

        async Task turnLoop(THHPlayer player)
        {
            if (!isRunning)
                return;
            for (int i = 0; i < 100; i++)
            {
                if (!isRunning)
                    return;
                if (turnTimer == null) //超时，结束回合
                    return;
                IResponse response = await answers.ask(player.id, new FreeActRequest(), option.timeoutForTurn * 2);
                switch (response)
                {
                    case UseResponse use:
                        Card card = getCard(use.cardId);
                        Card[] targets = getCards(use.targetsId);
                        if (!await player.tryUse(this, card, use.position, targets))
                            logger.log("Warning", "使用" + card + "失败");
                        break;
                    case AttackResponse attack:
                        card = getCard(attack.cardId);
                        Card target = getCard(attack.targetId);
                        Task<THHCard.AttackEventArg> task = card.tryAttack(this, player, target);
                        await task;
                        break;
                    case TurnEndResponse _:
                        return;
                    case SurrenderResponse _:
                        await surrender(player);
                        return;
                }
            }
        }

        public async Task turnEnd(THHPlayer player)
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new TurnEndEventArg() {player = player}, arg =>
            {
                logger.log("Debug", currentPlayer + "回合结束");

                List<Card> AllCharactor = new List<Card>();
                AllCharactor.Add(player.getMaster());
                AllCharactor.AddRange(player.getField());
                foreach (Card chara in AllCharactor)
                {
                    if (chara.isFreeze(this))
                    {
                        //chara.setAttackTimes(chara.getAttackTimes(this) + 1);
                        if ((chara.getMaxAttackTimes(this) - chara.getAttackTimes(this) >= 1) &&
                            chara.define is ServantCardDefine && chara.isReady(this))
                        {
                            logger.log("被冻结的随从解除冰冻");
                            chara.setFreeze(this, false);
                        }
                        else if ((chara.getMaxAttackTimes(this) - chara.getAttackTimes(this) >= 1) &&
                                 chara.define is MasterCardDefine)
                        {
                            logger.log("被冻结的英雄解除冰冻");
                            chara.setFreeze(this, false);
                        }
                    }
                }

                answers.cancel(answers.getRequests(player.id));
                return Task.CompletedTask;
            });
            time.cancel(turnTimer);
            turnTimer = null;
        }

        public class TurnEndEventArg : EventArg
        {
            public THHPlayer player;
        }

        /// <summary>
        /// 清理战场上死亡的随从
        /// </summary>
        /// <returns></returns>
        public Task updateDeath()
        {
            List<Card> deathList = new List<Card>();
            foreach (THHPlayer player in players)
            {
                if (player.getMaster().isDead(this))
                {
                    deathList.Add(player.getMaster());
                }

                if (player.getItem() != null && player.getItem().isDead(this))
                {
                    deathList.Add(player.getItem());
                }

                foreach (Card card in player.getField())
                {
                    if (card.isDead(this))
                    {
                        deathList.Add(card);
                    }
                }
            }

            if (deathList.Count > 0)
            {
                return deathList.die(this);
            }
            else
                return Task.CompletedTask;
        }

        internal Task surrender(THHPlayer player)
        {
            logger.log(player + "投降");
            return player.getMaster().die(this);
        }

        public async Task leave(THHPlayer player)
        {
            await player.getMaster().die(this);
        }

        internal async Task gameEnd(THHPlayer[] winners)
        {
            if (!isRunning)
                return;
            logger.log("Debug", "游戏结束，" + string.Join("，", winners.Select(p => p.ToString())) + "获得游戏胜利");
            await triggers.doEvent(new GameEndEventArg() {winners = winners});
            close();
        }

        public class GameEndEventArg : EventArg
        {
            public THHPlayer[] winners;
        }

        public void close()
        {
            isRunning = false;
            answers.cancelAll();
            if (turnTimer != null)
            {
                time.cancel(turnTimer);
                turnTimer = null;
            }

            onEnd?.Invoke();
        }

        public event Action<int, IResponse> onPlayerResponse;
        public event Action onEnd;

        public void logPlayerAction(int playerID, IResponse response)
        {
            onPlayerResponse?.Invoke(playerID, response);
        }

        #endregion

        public void Dispose()
        {
            close();
            if (answers != null)
                answers.Dispose();
            if (triggers != null)
                triggers.Dispose();
            if (time != null)
                time.Dispose();
        }
    }

    [Serializable]
    public class GameOption
    {
        public static GameOption Default => new GameOption();
        public int randomSeed = 0;
        public int[] sortedPlayers = null;
        public bool shuffle = true;
        public float timeoutForInitReplace = 30;
        public float timeoutForTurn = 75;
        public int startGem = 0;

        public GameOption()
        {
        }

        public GameOption(GameOption copyTarget)
        {
            randomSeed = copyTarget.randomSeed;
            sortedPlayers = copyTarget.sortedPlayers;
            shuffle = copyTarget.shuffle;
            timeoutForInitReplace = copyTarget.timeoutForInitReplace;
            timeoutForTurn = copyTarget.timeoutForTurn;
        }


        [Serializable]
        public class NotYourTurnException : Exception
        {
            public NotYourTurnException()
            {
            }

            public NotYourTurnException(Player player) : base("当前回合不是" + player + "的回合")
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

            public AlreadyPreparedException(Player player) : base(player + "已经准备过了")
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
    }
    
    public class HearthStoneOption : IGameOption
    {
        public struct PlayerInfo
        {
            public int id;
            public string name;
            public int master;
            public int[] deck;

            public PlayerInfo(int id, string name, int master, int[] deck)
            {
                this.id = id;
                this.name = name;
                this.master = master;
                this.deck = deck;
            }
        }
        public static HearthStoneOption Default => new HearthStoneOption();
        public int randomSeed = 0;
        public int[] sortedPlayers = null;
        public bool shuffle = true;
        public float timeoutForInitReplace = 30;
        public float timeoutForTurn = 75;
        public int startGem = 0;
        public PlayerInfo[] players = null;

        public HearthStoneOption()
        {
        }

        public HearthStoneOption(HearthStoneOption copyTarget)
        {
            randomSeed = copyTarget.randomSeed;
            sortedPlayers = copyTarget.sortedPlayers;
            shuffle = copyTarget.shuffle;
            timeoutForInitReplace = copyTarget.timeoutForInitReplace;
            timeoutForTurn = copyTarget.timeoutForTurn;
            players = copyTarget.players;
        }
    }
    public partial class HeathStoneRule : Rule
    {
        public HeathStoneRule(CardDefine[] defines) : base(defines)
        {
        }

        public override void onGameInit(CardEngine game, IGameOption options)
        {
            game.option = options;
            var option = getOption(game);
            foreach (var playerInfo in option.players)
            {
                // createPlayer()
            }
        }

        public override void onGameRun(CardEngine game)
        {
            throw new NotImplementedException();
        }

        public HearthStoneOption getOption(CardEngine game) => game.option as HearthStoneOption;
    }
}