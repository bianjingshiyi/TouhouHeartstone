using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

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
        #endregion
        #region AskAndAnswer
        public override void onAnswer(IResponse response)
        {
            base.onAnswer(response);
            THHPlayer player = getPlayer(response.playerId);
            switch (response)
            {
                case InitReplaceResponse initReplace:
                    _ = player.initReplace(this, getCards(initReplace.cardsId));
                    break;
                case UseResponse use:
                    Card card = getCard(use.cardId);
                    Card[] targets = getCards(use.targetsId);
                    _ = player.tryUse(this, card, use.position, targets);
                    break;
                case TurnEndResponse _:
                    _ = turnEnd(player);
                    break;
                case AttackResponse attack:
                    card = getCard(attack.cardId);
                    Card target = getCard(attack.targetId);
                    _ = card.tryAttack(this, target);
                    break;
                default:
                    logger.log("Warning", "未处理的响应：" + response);
                    break;
            }
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
        /// 游戏初始化
        /// </summary>
        public async Task init()
        {
            await triggers.doEvent(new InitEventArg(), arg =>
            {
                logger.log("Debug", "游戏初始化");
                //决定玩家行动顺序
                if (option.sortedPlayers == null)
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
                    Card[] cards = sortedPlayers[i].deck[sortedPlayers[i].deck.count - count, sortedPlayers[i].deck.count - 1];
                    sortedPlayers[i].deck.moveTo(cards, sortedPlayers[i].init, 0);
                }
                //logger.log("Debug", "游戏初始化，玩家行动顺序：" + string.Join("、", sortedPlayers.Select(p => p.ToString())) + "，"
                //    + "初始卡牌：" + string.Join("；", sortedPlayers.Select(p => string.Join("、", p.init.Select(c => c.ToString())))));
                return Task.CompletedTask;
            });
            startTimeout(option.timeout, async () =>
            {
                foreach (THHPlayer player in players.Where(p => !p.isPrepared))
                {
                    await player.initReplace(this);
                }
            });
        }
        public class InitEventArg : EventArg
        {
        }
        public THHPlayer[] sortedPlayers { get; private set; }
        public async Task start()
        {
            cancelTimeout();
            await triggers.doEvent(new StartEventArg(), arg =>
            {
                logger.log("Debug", "游戏开始");
                foreach (THHPlayer player in sortedPlayers)
                {
                    player.init.moveTo(player.init[0, player.init.count - 1], player.hand, 0);
                }
                return Task.CompletedTask;
            });
            await turnStart(sortedPlayers[0]);
        }
        public class StartEventArg : EventArg
        {
        }
        public THHPlayer currentPlayer { get; private set; }
        async Task turnStart(THHPlayer player)
        {
            await triggers.doEvent(new TurnStartEventArg() { player = player }, async arg =>
            {
                logger.log("Debug", arg.player + "的回合开始");
                //玩家的最大能量加1但是不超过10，充满玩家的能量。
                currentPlayer = arg.player;
                await arg.player.setMaxGem(this, arg.player.maxGem + 1);
                await arg.player.setGem(this, arg.player.maxGem);
                //抽一张牌
                await arg.player.draw(this);
                //使随从可以攻击
                foreach (Card card in player.field)
                {
                    card.setReady(true);
                    card.setAttackTimes(0);
                }
            });
            startTimeout(option.timeout, async () =>
            {
                await turnEnd(currentPlayer);
            });
        }
        public class TurnStartEventArg : EventArg
        {
            public THHPlayer player;
        }
        public async Task turnEnd(THHPlayer player)
        {
            if (currentPlayer != player)
                throw new NotYourTurnException(player);
            await triggers.doEvent(new TurnEndEventArg() { player = player }, arg =>
            {
                logger.log("Debug", currentPlayer + "回合结束");
                currentPlayer = null;
                return Task.CompletedTask;
            });
            cancelTimeout();
            await turnStart(getPlayerForNextTurn(player));
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
            List<THHPlayer> deathOwnerList = new List<THHPlayer>();
            foreach (THHPlayer player in players)
            {
                if (player.master.getCurrentLife() <= 0)
                {
                    deathList.Add(player.master);
                    deathOwnerList.Add(player);
                }
                foreach (Card card in player.field)
                {
                    if (card.getCurrentLife() <= 0)
                    {
                        deathList.Add(card);
                        deathOwnerList.Add(player);
                    }
                }
            }
            //TODO:死亡结算顺序。
            return deathList.ToArray().die(this, deathOwnerList.ToArray());
        }
        public Task gameEnd(THHPlayer[] winners)
        {
            logger.log("Debug", "游戏结束，" + string.Join("，", winners.Select(p => p.ToString())) + "获得游戏胜利");
            return triggers.doEvent(new GameEndEventArg() { winners = winners });
        }
        public class GameEndEventArg : EventArg
        {
            public THHPlayer[] winners;
        }
        public void close()
        {
            cancelTimeout();
        }
        #endregion
        #region Timeout
        CancellationTokenSource timeoutCTS { get; set; } = null;
        private void startTimeout(float sec, Action onTimeout)
        {
            timeoutCTS = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay((int)(sec * 1000));
                if (timeoutCTS == null || timeoutCTS.IsCancellationRequested)
                    return;
                logger.log("Debug", currentPlayer + "超时");
                this.onTimeout?.Invoke();
                onTimeout?.Invoke();
            }, timeoutCTS.Token);
        }
        public event Action onTimeout;
        private void cancelTimeout()
        {
            if (timeoutCTS == null)
                return;
            timeoutCTS.Cancel();
            timeoutCTS.Dispose();
            timeoutCTS = null;
        }
        #endregion
        [Obsolete]
        public void use(int playerIndex, int cardRID, int targetPosition, int[] targetCardsRID)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        public void attack(int playerIndex, int cardRID, int targetCardRID)
        {
            throw new NotImplementedException();
        }
        public void turnEnd(int playerIndex)
        {
            throw new NotImplementedException();
        }
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
}