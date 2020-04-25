using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UberLogger;

namespace TouhouHeartstone
{
    public class THHGame : CardEngine, IDisposable
    {
        public GameOption option { get; }
        public CardEngine engine { get; }
        Dictionary<Player, IFrontend> dicPlayerFrontend { get; } = new Dictionary<Player, IFrontend>();
        public THHGame(params CardDefine[] defines) : base(null, null, GameOption.Default.randomSeed, defines)
        {
            option = GameOption.Default;
        }
        public THHGame(GameOption option, params CardDefine[] defines) : base(null, null, GameOption.Default.randomSeed, defines)
        {
            this.option = option;
        }
        [Obsolete]
        public THHGame(IGameEnvironment env, bool shuffle = true, params CardDefine[] cards) : base(env, new HeartStoneRule(env), (int)DateTime.Now.ToBinary())
        {
            engine = new CardEngine(env, new HeartStoneRule(env), (int)DateTime.Now.ToBinary());
            engine.setProp("shuffle", shuffle);
            engine.afterEvent += afterEvent;
        }
        #region Player
        public THHPlayer createPlayer(int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck)
        {
            if (players.Any(p => p.id == id))
                throw new ArgumentException("已经存在ID为" + id + "的玩家");
            THHPlayer player = new THHPlayer(this, id, name, master, deck);
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
        public THHPlayer getOpponent(THHPlayer player)
        {
            return players.FirstOrDefault(p => p != player);
        }
        #endregion
        #region Card
        public override Card createCard(CardDefine define)
        {
            Card card = base.createCard(define);
            if (define is ServantCardDefine servant)
            {
                foreach (string keyword in servant.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    switch (keyword)
                    {
                        case Keyword.TAUNT:
                            card.setTaunt(true);
                            break;
                        case Keyword.CHARGE:
                            card.setCharge(true);
                            break;
                        default:
                            throw new UnknowKeywordException("未知关键词" + keyword);
                    }
                }
            }
            return card;
        }
        public Card[] findAllCardsInField(Func<Card, bool> filter)
        {
            List<Card> cardList = new List<Card>();
            foreach (THHPlayer player in players)
            {
                if (filter == null || filter.Invoke(player.master))
                    cardList.Add(player.master);
                foreach (Card servant in player.field)
                {
                    if (filter == null || filter.Invoke(servant))
                        cardList.Add(servant);
                }
            }
            return cardList.ToArray();
        }
        #endregion
        public int registerCardDefine(CardDefine define)
        {
            return engine.rule.pool.register(define);
        }
        public int[] getUsableCards(int playerIndex)
        {
            return engine.getPlayerAt(playerIndex)["Hand"].Concat(engine.getPlayerAt(playerIndex)["Skill"]).
                   Where(c => { return string.IsNullOrEmpty(isUsable(c)); }).
                   Select(c => { return c.id; }).ToArray();
        }
        public string isUsable(int rid)
        {
            return isUsable(engine.getCard(rid));
        }
        private string isUsable(Card card)
        {
            Player player = card.pile.owner;
            //通用规则
            if (player.getProp<int>("gem") < card.define.getProp<int>("cost"))//法力值不足不能用
                return "Unusable_NotEnoughGem";
            //if ((card.define.type == CardDefineType.spell ||
            //     card.define.type == CardDefineType.skill) &&
            //     getTargets(rid, "onUse").Length < 1)//法术，技能卡没有合适的目标不可以用
            //    return "Unusable_NoValidTarget";
            //卡片自己的规则
            return card.define.isUsable(engine, player, card);
        }
        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="frontend">实现接口的前端对象，Game会通过这个对象与前端进行沟通。</param>
        /// <param name="deck">玩家使用的卡组，数组的第一个整数代表玩家使用的角色卡，后30个是玩家使用的卡组。</param>
        public void addPlayer(IFrontend frontend, int[] deck)
        {
            throw new NotImplementedException();
        }
        #region Gameflow
        /// <summary>
        /// 运行游戏
        /// </summary>
        /// <returns>游戏运行的Task</returns>
        public Task run()
        {
            cts = new CancellationTokenSource();
            Task task = Task.Run(gameflow, cts.Token);
            return task;
        }
        CancellationTokenSource cts { get; set; } = null;
        private async Task gameflow()
        {
            await init();
            foreach (var result in await answers.askAll(sortedPlayers.Select(p => p.id).ToArray(), new InitReplaceRequest()
            {
            }, option.timeout))
            {
                THHPlayer player = getPlayer(result.Key);
                await player.initReplace(this, getCards((result.Value as InitReplaceResponse).cardsId));
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
            if (cts == null || cts.IsCancellationRequested)
                return;
            await triggers.doEvent(new InitEventArg(), arg =>
            {
                //决定玩家行动顺序
                if (option.sortedPlayers == null || option.sortedPlayers.Length != players.Length)
                {
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
                    this.sortedPlayers = option.sortedPlayers.Select(id => players.FirstOrDefault(p => p.id == id)).ToArray();
                //创建主人公和技能卡
                Card[] masterCards = sortedPlayers.Select(p => { return p.master; }).ToArray();
                foreach (Card card in masterCards)
                {
                    card.setCurrentLife(30);
                }
                //洗牌，然后抽初始卡牌
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    if (option.shuffle)
                        sortedPlayers[i].deck.shuffle(this);
                    int count = i == 0 ? 3 : 4;
                    var cards = sortedPlayers[i].deck[sortedPlayers[i].deck.count - count, sortedPlayers[i].deck.count - 1].Reverse();
                    sortedPlayers[i].deck.moveTo(this, cards, sortedPlayers[i].init, 0);
                }
                logger.log("Debug", "游戏初始化，玩家行动顺序：" + string.Join("、", sortedPlayers.Select(p => p.ToString())) + "，"
                    + "初始卡牌：" + string.Join("；", sortedPlayers.Select(p => string.Join("、", p.init.Select(c => c.ToString())))));
                return Task.CompletedTask;
            });
        }
        public class InitEventArg : EventArg
        {
        }
        public THHPlayer[] sortedPlayers { get; private set; }
        internal async Task start()
        {
            if (cts == null || cts.IsCancellationRequested)
                return;
            await triggers.doEvent(new StartEventArg(), arg =>
            {
                logger.log("Debug", "游戏开始");
                foreach (THHPlayer player in sortedPlayers)
                {
                    player.init.moveTo(this, player.init[0, player.init.count - 1], player.hand, 0);
                }
                return Task.CompletedTask;
            });
        }
        public class StartEventArg : EventArg
        {
        }
        public THHPlayer currentPlayer { get; private set; }
        async Task turnStart(THHPlayer player)
        {
            if (cts == null || cts.IsCancellationRequested)
                return;
            await triggers.doEvent(new TurnStartEventArg() { player = player }, async arg =>
            {
                logger.log("Debug", arg.player + "的回合开始");
                //玩家的最大能量加1但是不超过10，充满玩家的能量。
                await arg.player.setMaxGem(this, arg.player.maxGem + 1);
                await arg.player.setGem(this, arg.player.maxGem);
                //抽一张牌
                await arg.player.draw(this);
                //重置技能
                player.skill.setUsed(false);
                //使随从可以攻击
                foreach (Card card in player.field)
                {
                    card.setReady(true);
                    card.setAttackTimes(0);
                }
            });
        }
        public class TurnStartEventArg : EventArg
        {
            public THHPlayer player;
        }
        async Task turnLoop(THHPlayer player)
        {
            if (cts == null || cts.IsCancellationRequested)
                return;
            for (int i = 0; i < 100; i++)
            {
                if (cts == null || cts.IsCancellationRequested)
                    return;
                switch (await answers.ask(player.id, new FreeActRequest(), option.timeout))
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
                        await card.tryAttack(this, target);
                        break;
                    case TurnEndResponse _:
                        return;
                }
            }
        }
        public async Task turnEnd(THHPlayer player)
        {
            if (cts == null || cts.IsCancellationRequested)
                return;
            await triggers.doEvent(new TurnEndEventArg() { player = player }, arg =>
            {
                logger.log("Debug", currentPlayer + "回合结束");
                return Task.CompletedTask;
            });
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
            Dictionary<Card, THHCard.DeathEventArg.Info> deathDic = new Dictionary<Card, THHCard.DeathEventArg.Info>();
            foreach (THHPlayer player in players)
            {
                if (player.master.getCurrentLife() <= 0)
                {
                    deathDic.Add(player.master, new THHCard.DeathEventArg.Info()
                    {
                        card = player.master,
                        player = player,
                        position = 0
                    });
                }
                foreach (Card card in player.field)
                {
                    if (card.getCurrentLife() <= 0)
                    {
                        deathDic.Add(card, new THHCard.DeathEventArg.Info()
                        {
                            card = card,
                            player = player,
                            position = player.field.indexOf(card)
                        });
                    }
                }
            }
            if (deathDic.Count > 0)
            {
                logger.log("结算" + string.Join("，", deathDic.Keys) + "的死亡");
                return deathDic.Keys.die(this, deathDic);
            }
            else
                return Task.CompletedTask;
        }
        internal async Task gameEnd(THHPlayer[] winners)
        {
            if (cts == null || cts.IsCancellationRequested)
                return;
            logger.log("Debug", "游戏结束，" + string.Join("，", winners.Select(p => p.ToString())) + "获得游戏胜利");
            await triggers.doEvent(new GameEndEventArg() { winners = winners });
            answers.cancelAll();
            cts = null;
        }
        public class GameEndEventArg : EventArg
        {
            public THHPlayer[] winners;
        }
        public void close()
        {
            answers.cancelAll();
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
        }
        #endregion
        private void afterEvent(Event @event)
        {
            if (@event.parent == null)
            {
                foreach (Player player in engine.getPlayers())
                {
                    EventWitness[] wArray = generateWitnessTree(engine, player, @event);
                    for (int i = 0; i < wArray.Length; i++)
                    {
                        dicPlayerFrontend[player].sendWitness(wArray[i]);
                    }
                }
            }
        }
        EventWitness[] generateWitnessTree(CardEngine engine, Player player, Event e)
        {
            List<EventWitness> wlist = new List<EventWitness>();
            if (e is VisibleEvent)
            {
                EventWitness w = (e as VisibleEvent).getWitness(engine, player);
                for (int i = 0; i < e.before.Count; i++)
                {
                    wlist.AddRange(generateWitnessTree(engine, player, e.before[i]));
                }
                for (int i = 0; i < e.child.Count; i++)
                {
                    w.child.AddRange(generateWitnessTree(engine, player, e.child[i]));
                }
                wlist.Add(w);
                for (int i = 0; i < e.after.Count; i++)
                {
                    wlist.AddRange(generateWitnessTree(engine, player, e.after[i]));
                }
                return wlist.ToArray();
            }
            else
            {
                for (int i = 0; i < e.child.Count; i++)
                {
                    wlist.AddRange(generateWitnessTree(engine, player, e.child[i]));
                }
                return wlist.ToArray();
            }
        }
        public void Dispose()
        {
            close();
        }
    }
    [Serializable]
    public class GameOption
    {
        public static GameOption Default { get; } = new GameOption();
        public int randomSeed = 0;
        public int[] sortedPlayers = null;
        public bool shuffle = true;
        public float timeout = 30;
    }

    [Serializable]
    public class NotYourTurnException : Exception
    {
        public NotYourTurnException() { }
        public NotYourTurnException(Player player) : base("当前回合不是" + player + "的回合")
        {
        }
        public NotYourTurnException(string message) : base(message) { }
        public NotYourTurnException(string message, Exception inner) : base(message, inner) { }
        protected NotYourTurnException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class AlreadyPreparedException : Exception
    {
        public AlreadyPreparedException() { }
        public AlreadyPreparedException(Player player) : base(player + "已经准备过了")
        {
        }
        public AlreadyPreparedException(string message) : base(message) { }
        public AlreadyPreparedException(string message, Exception inner) : base(message, inner) { }
        protected AlreadyPreparedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class UnknowKeywordException : Exception
    {
        public UnknowKeywordException() { }
        public UnknowKeywordException(string message) : base(message) { }
        public UnknowKeywordException(string message, Exception inner) : base(message, inner) { }
        protected UnknowKeywordException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}