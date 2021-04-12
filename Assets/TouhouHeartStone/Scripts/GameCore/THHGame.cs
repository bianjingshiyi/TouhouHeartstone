using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone.Builtin;

namespace TouhouHeartstone
{
    public class THHGame : CardEngine, IDisposable
    {
        public static HearthStoneRule rule = new HearthStoneRule(null);
        public HearthstoneOption getOption() => this.option as HearthstoneOption;

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

        public THHGame(params CardDefine[] defines) : base(HearthstoneOption.Default.randomSeed, defines)
        {
            option = HearthstoneOption.Default;
        }

        public THHGame(HearthstoneOption option, params CardDefine[] defines) : base(option.randomSeed, defines)
        {
            this.option = option;
        }

        public THHGame() : base ()
        {
            
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
        public Player createPlayer(int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck)
        {
            return rule.createPlayer(this, id, name, master.id, deck.Select(d=>d.id).ToArray()) as Player;
        }

        public Player[] players
        {
            get { return base.players.Cast<Player>().ToArray(); }
        }

        public Player getPlayer(int id)
        {
            return rule.getPlayer(this, id) as Player;
        }

        /// <summary>
        /// 获取下一个行动的玩家。
        /// </summary>
        /// <param name="lastPlayer"></param>
        /// <returns></returns>
        public Player getPlayerForNextTurn(Player lastPlayer)
        {
            int index = Array.IndexOf(getSortedPlayers(), lastPlayer);
            if (index < 0)
                throw new IndexOutOfRangeException(lastPlayer + "不在玩家行动队列中");
            index++;
            if (index >= getSortedPlayers().Length)
                index = 0;
            return getSortedPlayers()[index];
        }

        /// <summary>
        /// 获取玩家的对手。
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Player getOpponent(TouhouCardEngine.Player player)
        {
            return players.FirstOrDefault(p => p != player);
        }

        public Player[] getAllPlayer()
        {
            return players;
        }

        #endregion

        #region Card

        public Card createCard(CardDefine define)
        {
            return rule.createCard(this, define);
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
            foreach (Player player in players)
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

        public Card[] getAllEnemies(Player player)
        {
            Player opponent = getOpponent(player);
            return opponent.getField().Concat(new Card[] {opponent.getMaster()}).ToArray();
        }

        public Card[] getAllAllys(Player player)
        {
            return player.getField().Concat(new Card[] {player.getMaster()}).ToArray();
        }



        #endregion

        #region Gameflow
        
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
            Task<Dictionary<int, IResponse>> task = answers.askAll(getSortedPlayers().Select(p => p.id).ToArray(),
                new InitReplaceRequest()
                {
                }, getOption().timeoutForInitReplace);
            onInited?.Invoke();
            Dictionary<int, IResponse> initReplaceResponses = await task;
            if (!isRunning)
                return;
            foreach (var result in initReplaceResponses)
            {
                Player player = getPlayer(result.Key);
                if (result.Value is InitReplaceResponse initReplace)
                    await player.initReplace(this, getCards(initReplace.cardsId));
                else if (result.Value is SurrenderResponse surrender)
                    await this.surrender(player);
            }

            await start();
            setCurrentPlayer(getSortedPlayers()[0]);
            for (int i = 0; i < 100; i++)
            {
                await turnStart(getCurrentPlayer());
                await turnLoop(getCurrentPlayer());
                await turnEnd(getCurrentPlayer());
                setCurrentPlayer(getPlayerForNextTurn(getCurrentPlayer()));
            }

            await gameEnd(new Player[0]);
        }

        internal async Task init()
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new HearthStoneRule.InitEventArg(), async arg =>
            {
                //决定玩家行动顺序
                if (getOption().sortedPlayers == null || getOption().sortedPlayers.Length != players.Length)
                {
                    if (getOption().sortedPlayers != null && getOption().sortedPlayers.Length != players.Length)
                        logger?.log("Warning", "游戏参数玩家行动顺序长度与实际数量不匹配");
                    int playerCount = players.Count();
                    foreach (var player in players)
                    {
                        player.setProp(PLAYER.ORDER, randomInt(1, playerCount--));
                    }
                }
                else
                {
                    int order = 1;
                    foreach (var id in getOption().sortedPlayers)
                    {
                        players.FirstOrDefault(p => p.id == id).setProp(PLAYER.ORDER, order++);
                    }
                }
                    

                //创建主人公和技能卡
                Card[] masterCards = getSortedPlayers().Select(p => { return p.getMaster(); }).ToArray();
                foreach (Card card in masterCards)
                {
                    await card.setCurrentLife(this, 30);
                }

                //洗牌，然后抽初始卡牌
                for (int i = 0; i < getSortedPlayers().Length; i++)
                {
                    if (getSortedPlayers()[i].getDeck().count < 1)
                        continue;
                    if (getOption().shuffle)
                        getSortedPlayers()[i].getDeck().shuffle(this);
                    int count = i == 0 ? 3 : 4;
                    var cards = getSortedPlayers()[i].getDeck().Skip(getSortedPlayers()[i].getDeck().count - count).Reverse()
                        .ToArray();
                    await getSortedPlayers()[i].getDeck().moveTo(this, cards, getSortedPlayers()[i].getInit());
                }

                logger.log("Debug", "游戏初始化，玩家行动顺序："
                                    + string.Join("、", getSortedPlayers().Select(p => p.ToString())) + "，"
                                    + "玩家英雄：" + string.Join("，",
                                        getSortedPlayers().Select(p => p.getMaster() + "(" + p.getSkill() + ")")) + "，"
                                    + "初始卡牌：" + string.Join("；",
                                        getSortedPlayers().Select(p =>
                                            string.Join("、", p.getInit().Select(c => c.ToString())))));
            });
        }

        public Player[] getSortedPlayers() => players.OrderBy(player => player.getOrder()).ToArray();

        internal async Task start()
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new HearthStoneRule.StartEventArg(), async arg =>
            {
                logger.log("Debug", "游戏开始");
                foreach (Player player in getSortedPlayers())
                {
                    await player.setMaxGem(this, getOption().startGem);
                    await player.getInit().moveTo(this, player.getInit()[0, player.getInit().count - 1],
                        player.getHand(), 0);
                    if (player != getSortedPlayers()[0])
                    {
                        await player.getHand().add(this, createCard(getDefine<LuckyCoin>()));
                        logger.log("由于后手行动" + player + "获得一张幸运币");
                    }

                    arg.startHandDic.Add(player, new HearthStoneRule.StartEventArg.StartInfo()
                    {
                        gem = player.getMaxGem(),
                        hands = player.getHand().ToArray()
                    });
                }
            });
        }

        private void setCurrentPlayer(Player value) => setProp(GAME.CURRENTPLAYER, value);
        public Player getCurrentPlayer() => getProp<Player>(GAME.CURRENTPLAYER);

        async Task turnStart(Player player)
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new HearthStoneRule.TurnStartEventArg() {player = player}, async arg =>
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
            turnTimer = time.startTimer(getOption().timeoutForTurn);
            turnTimer.onExpired += onTurnTimeout;
        }

        public ITimer turnTimer { get; set; } = null;

        private void onTurnTimeout()
        {
            logger.log("Debug", getCurrentPlayer() + "回合超时");
            turnTimer = null;
            answers.cancel(answers.getRequests(getCurrentPlayer().id));
        }

        async Task turnLoop(Player player)
        {
            if (!isRunning)
                return;
            for (int i = 0; i < 100; i++)
            {
                if (!isRunning)
                    return;
                if (turnTimer == null) //超时，结束回合
                    return;
                IResponse response = await answers.ask(player.id, new FreeActRequest(), getOption().timeoutForTurn * 2);
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

        public async Task turnEnd(Player player)
        {
            if (!isRunning)
                return;
            await triggers.doEvent(new HearthStoneRule.TurnEndEventArg() {player = player}, arg =>
            {
                logger.log("Debug", getCurrentPlayer() + "回合结束");

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

        /// <summary>
        /// 清理战场上死亡的随从
        /// </summary>
        /// <returns></returns>
        public Task updateDeath()
        {
            List<Card> deathList = new List<Card>();
            foreach (Player player in players)
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

        internal Task surrender(Player player)
        {
            logger.log(player + "投降");
            return player.getMaster().die(this);
        }

        public async Task leave(Player player)
        {
            await player.getMaster().die(this);
        }

        internal async Task gameEnd(Player[] winners)
        {
            if (!isRunning)
                return;
            logger.log("Debug", "游戏结束，" + string.Join("，", winners.Select(p => p.ToString())) + "获得游戏胜利");
            await triggers.doEvent(new HearthStoneRule.GameEndEventArg() {winners = winners});
            close();
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
    public class HearthstoneOption : GameOption
    {
        public static HearthstoneOption Default => new HearthstoneOption();
        // public int randomSeed = 0;
        public int[] sortedPlayers = null;
        public bool shuffle = true;
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
            timeoutForInitReplace = copyTarget.timeoutForInitReplace;
            timeoutForTurn = copyTarget.timeoutForTurn;
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
    }
    
    public partial class HearthStoneRule : Rule
    {
        public HearthStoneRule(CardDefine[] defines) : base(defines)
        {
        }

        public override void onGameInit(CardEngine game, GameOption options, RoomPlayerInfo[] players)
        {
            game.option = options;
            var option = getOption(game);
            foreach (var playerInfo in players)
            {
                var deck = playerInfo.getProp<int[]>("deck");
                createPlayer(game, playerInfo.RoomID, playerInfo.name, deck[0], deck.Skip(1).ToArray());
            }
        }

        public override void onGameRun(CardEngine game)
        {
            throw new NotImplementedException();
        }

        public HearthstoneOption getOption(CardEngine game) => game.option as HearthstoneOption;
        
        public TouhouCardEngine.Player createPlayer(CardEngine game, int id, string name, int master, int[] deck)
        {
            if (game.getDefine(master) is MasterCardDefine masterCardDefine)
            {
                
            }
            else
            {
                masterCardDefine = new Reimu();
                game.logger.logError("非法角色ID" + master + "被替换为灵梦");
            }

            int luckyCoinId = new LuckyCoin().id;
            for (int i = 1; i < deck.Length; i++)
            {
                if (game.getDefine(deck[i]) == null)
                {
                    game.logger.logError("非法卡牌ID" + deck[i] + "被替换为幸运币");
                    deck[i] = luckyCoinId;
                }
            }
            return createPlayer(game,id,name,masterCardDefine,game.getDefines(deck));
        }
        
        public TouhouCardEngine.Player createPlayer(CardEngine game, int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck)
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
            player[PILE.MASTER].add(game, createCard(game, master));
            player[PILE.SKILL].add(game, createCard(game, game.getDefine(master.skillID)));
            player[PILE.DECK].add(game, deck.Select(d => createCard(game, d)));
            player.setIsPrepared(false);
            player.setGem(game, 0);
            player.setMaxGem(game, 0);
            player.setFatigue(0);
            game.addPlayer(player);
            return player;
        }
        
        public TouhouCardEngine.Player getPlayer(CardEngine game, int id)
        {
            return game.players.FirstOrDefault(p => p.id == id);
        }
        
        public Card createCard(CardEngine game, CardDefine define)
        {
            Card card = game.issueCard(define);
            if (define is MasterCardDefine master)
            {
                card.setLife(game, master.life);
            }
            else if (define is ServantCardDefine servant)
            {
                card.setCost(game, servant.cost);
                card.setAttack(game, servant.attack);
                card.setLife(game, servant.life);
                card.setSpellDamage(game, servant.spellDamage);

                foreach (string keyword in servant.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    card.setProp(game, keyword, true);
                }

                card.setProp(game, PROPERTY.KEYWORDS, servant.keywords);
                card.setProp(game, nameof(ServantCardDefine.tags), servant.tags);
            }
            else if (define is SpellCardDefine spell)
            {
                card.setCost(game, spell.cost);
                foreach (var keyword in spell.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    card.setProp(game, keyword, true);
                }

                card.setProp(game, PROPERTY.KEYWORDS, spell.keywords);
                card.setProp(game, nameof(SpellCardDefine.tags), spell.tags);
            }
            else if (define is SkillCardDefine skill)
            {
                card.setCost(game, skill.cost);
            }
            else if (define is ItemCardDefine item)
            {
                card.setCost(game, item.cost);
                card.setAttack(game, item.attack);
                card.setLife(game, item.life);
            }

            return card;
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
        }

        public class TurnStartEventArg : EventArg
        {
            public Player player;
        }

        public class TurnEndEventArg : EventArg
        {
            public Player player;
        }

        public class GameEndEventArg : EventArg
        {
            public Player[] winners;
        }

        public class InitReplaceEventArg : EventArg
        {
            public Player player;
            public Card[] cards;
            public Card[] replacedCards;
        }

        public class SetMaxGemEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public int value;
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
                return new object[] {
                    player.name,
                    value
                };
            }
        }

        public class BurnEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public Card card;
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
        }

        /// <summary>
        /// 选择事件，一般指从几个选项中选择一项
        /// 卡面描述："选择一项"
        /// </summary>
        public class ChooseOneEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public IEnumerable<Card> options;
            public Card choose;
            public bool isShow = true;

            public ICard getCard(IGame game, IPlayer viewer)
            {
                return choose;
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
            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_NotShow = "{0} 进行了选择";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (isShow)
                    return TEMPLATE;
                else
                    return TEMPLATE_NotShow;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    choose?.getFormatString()
                };
            }
            
        }

        public class UseEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public Card card;
            public int position;
            public int cost;
            public Card[] targets;
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
                return new object[] {
                    player.name,
                    card?.getFormatString(),
                    string.Join("，", targets?.Select(c => "{card:" + c.define.id + "}"))
                };
            }
        }

        public class EnterFieldEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public Pile from;
            public Card card;
            public int position;
            public int attack;
            public int life;
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
                return new object[] {
                    player.name,
                    card.getFormatString(),
                    position
                };
            }
        }

        public class RebornEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public int position;
            public Card card;
            public Pile pile;

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
                return new object[] {
                    player.name,
                    card.getFormatString()
                };
            }
        }

        public class RevealArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public Card card;  //被揭示的牌

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

            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_INVALID = "{0} 揭示了个寂寞";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return card == null ? TEMPLATE_INVALID : TEMPLATE_VALID;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card?.getFormatString()
                };
            }
        }
    }
}