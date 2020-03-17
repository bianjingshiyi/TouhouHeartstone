using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class THHGame : CardEngine
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
        public THHPlayer createPlayer(string name, MasterCardDefine master, IEnumerable<CardDefine> deck)
        {
            THHPlayer player = new THHPlayer(this, getNewPlayerId(), name, master, deck);
            addPlayer(player);
            return player;
        }
        public THHPlayer[] players
        {
            get { return getPlayers().Cast<THHPlayer>().ToArray(); }
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
        /// <summary>
        /// 游戏初始化
        /// </summary>
        public async Task init()
        {
            await triggers.doEvent(new InitEventArg(), arg =>
            {
                logger.log("Debug", "游戏初始化");
                //决定玩家行动顺序
                List<THHPlayer> remainedList = new List<THHPlayer>(players);
                THHPlayer[] sortedPlayers = new THHPlayer[remainedList.Count];
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    int index = randomInt(0, remainedList.Count - 1);
                    sortedPlayers[i] = remainedList[index];
                    remainedList.RemoveAt(index);
                }
                this.sortedPlayers = sortedPlayers;
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
        }
        public class InitEventArg : EventArg
        {
        }
        public THHPlayer[] sortedPlayers { get; private set; }
        [Obsolete]
        public void initReplace(int playerIndex, int[] cardsRID)
        {
            throw new NotImplementedException();
        }
        public async Task start()
        {
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
        CancellationTokenSource timeoutCTS { get; set; } = null;
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
            timeoutCTS = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(30 * 1000);
                if (timeoutCTS.IsCancellationRequested)
                    return;
                logger.log("Debug", currentPlayer + "超时");
                await turnEnd(currentPlayer);
            }, timeoutCTS.Token);
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
            timeoutCTS.Cancel();
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
    }
    [Serializable]
    public class GameOption
    {
        public static GameOption Default { get; } = new GameOption();
        public int randomSeed = 0;
        public bool shuffle = true;
    }
    public class THHPlayer : Player
    {
        public Card master { get; }
        public Card skill { get; }
        public Pile deck { get; }
        public Pile init { get; }
        public Pile hand { get; }
        public Pile field { get; }
        public Pile grave { get; }
        public bool isPrepared { get; set; } = false;
        public int gem { get; private set; } = 0;
        public int maxGem { get; private set; } = 0;
        public int fatigue { get; private set; } = 0;
        public THHPlayer(THHGame game, int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck) : base(id, name)
        {
            this.master = game.createCard(master);
            pileList.Add(new Pile("Master", new Card[] { this.master }, 1));
            skill = game.createCardById(master.skillID);
            pileList.Add(new Pile("Skill", new Card[] { skill }, 1));
            this.deck = new Pile("Deck", deck.Select(d => game.createCard(d)).ToArray());
            pileList.Add(this.deck);
            init = new Pile("Init", maxCount: 4);
            pileList.Add(init);
            hand = new Pile("Hand", maxCount: 10);
            pileList.Add(hand);
            field = new Pile("Field", maxCount: 7);
            pileList.Add(field);
            grave = new Pile("Grave");
            pileList.Add(grave);
        }
        public async Task initReplace(THHGame game, params Card[] cards)
        {
            if (isPrepared)
                throw new AlreadyPreparedException(this);
            await game.triggers.doEvent(new InitReplaceEventArg() { player = this, cards = cards }, arg =>
            {
                arg.replacedCards = arg.player.init.replaceByRandom(game, arg.cards, arg.player.deck);
                //玩家准备完毕
                arg.player.isPrepared = true;
                game.logger.log(arg.player + "替换卡牌：" + string.Join("，", arg.cards.Select(c => c.ToString())) + "=>"
                    + string.Join("，", arg.replacedCards.Select(c => c.ToString())));
                return Task.CompletedTask;
            });
            //判断是否所有玩家都准备完毕
            if (game.players.All(p => { return p.isPrepared; }))
            {
                //对战开始
                await game.start();
            }
        }
        public class InitReplaceEventArg : EventArg
        {
            public THHPlayer player;
            public Card[] cards;
            public Card[] replacedCards;
        }
        public Task setGem(THHGame game, int value)
        {
            return game.triggers.doEvent(new SetGemEventArg() { player = this, value = value }, arg =>
            {
                arg.player.gem = arg.value;
                if (arg.player.gem < 0)
                    arg.player.gem = 0;
                if (arg.player.gem > arg.player.maxGem)
                    arg.player.gem = arg.player.maxGem;
                game.logger.log(arg.player + "的法力水晶变为" + arg.player.gem);
                return Task.CompletedTask;
            });
        }
        public class SetGemEventArg : EventArg
        {
            public THHPlayer player;
            public int value;
        }
        public Task setMaxGem(THHGame game, int value)
        {
            return game.triggers.doEvent(new SetMaxGemEventArg() { player = this, value = value }, arg =>
            {
                arg.player.maxGem = arg.value;
                if (arg.player.maxGem < 0)
                    arg.player.maxGem = 0;
                if (arg.player.maxGem > 10)
                    arg.player.maxGem = 10;
                if (arg.player.gem > arg.player.maxGem)
                    arg.player.gem = 0;
                game.logger.log(arg.player + "的法力水晶上限变为" + arg.player.maxGem);
                return Task.CompletedTask;
            });
        }
        public class SetMaxGemEventArg : EventArg
        {
            public THHPlayer player;
            public int value;
        }
        public Task draw(THHGame game)
        {
            if (deck.count < 1)//无牌可抽，疲劳！
            {
                return game.triggers.doEvent(new FatigueEventArg() { player = this }, arg =>
                {
                    arg.player.fatigue++;
                    game.logger.log(arg.player + "已经没有卡牌了，当前疲劳值：" + arg.player.fatigue);
                    return arg.player.master.damage(game, arg.player.fatigue);
                });
            }
            else if (hand.count >= hand.maxCount)
            {
                return game.triggers.doEvent(new BurnEventArg() { player = this }, arg =>
                {
                    arg.card = arg.player.deck.top;
                    arg.player.deck.moveTo(arg.card, arg.player.grave, arg.player.grave.count);
                    game.logger.log(arg.player + "的手牌已经满了，" + arg.card + "被送入墓地");
                    return Task.CompletedTask;
                });
            }
            else
            {
                return game.triggers.doEvent(new DrawEventArg() { player = this }, arg =>
                {
                    arg.card = arg.player.deck.top;
                    arg.player.deck.moveTo(arg.card, arg.player.hand, arg.player.hand.count);
                    game.logger.log(arg.player + "抽" + arg.card);
                    return Task.CompletedTask;
                });
            }
        }
        public class DrawEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
        }
        public class BurnEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
        }
        public class FatigueEventArg : EventArg
        {
            public THHPlayer player;
        }
        public async Task<bool> tryUse(THHGame game, Card card, int position, params Card[] targets)
        {
            if (game.currentPlayer != this)//不是你的回合
                return false;
            if (card.define is ServantCardDefine servant)
            {
                if (gem < card.getCost())//费用不够
                    return false;
            }
            else if (card.define is SpellCardDefine spell)
            {
                if (gem < card.getCost())
                    return false;
            }
            else
            {
                return false;//不知道是什么卡
            }
            await setGem(game, gem - card.getCost());
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, targets = targets }, async arg =>
            {
                game.logger.log(arg.player + "使用" + arg.card);
                if (card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.servant))
                {
                    //随从卡，将卡置入战场
                    await trySummon(game, arg.player.hand, arg.card, arg.position);
                    Effect effect = arg.card.define.getEffectOn<BattleCryEventArg>();
                    if (effect != null)
                    {
                        await game.triggers.doEvent(new BattleCryEventArg() { player = arg.player, card = arg.card, effect = effect, targets = arg.targets }, arg2 =>
                        {
                            return arg2.effect.execute(game, arg2.player, arg2.card, new object[0], arg2.targets);
                        });
                    }
                }
                else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.spell))
                {
                    //法术卡，释放效果然后丢进墓地
                    //player["Hand"].moveTo(card, player["Warp"], player["Warp"].count);
                    //if (card.define.effects != null && card.define.effects.Length > 0)
                    //{
                    //    Effect effect = card.define.effects.FirstOrDefault(e => { return card.pile.name == e.pile && e.trigger == "onUse"; });
                    //    if (effect != null)
                    //        effect.execute(engine, player, card, targetCards);
                    //}
                    //player["Warp"].moveTo(card, player["Grave"], player["Grave"].count);
                }
            });
            return true;
        }
        public class UseEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
            public int position;
            public Card[] targets;
        }
        public class BattleCryEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
            public Effect effect;
            public Card[] targets;
        }
        public async Task<bool> trySummon(THHGame game, Pile from, Card card, int position)
        {
            if (field.count >= field.maxCount)//没位置了
                return false;
            await game.triggers.doEvent(new SummonEventArg() { player = this, from = from, card = card, position = position }, arg =>
            {
                game.logger.log(arg.player + "从" + arg.from + "召唤" + arg.card + "，位于" + arg.position);
                arg.from.moveTo(arg.card, arg.player.field, arg.position);
                if (card.define is ServantCardDefine servant)
                {
                    card.setCurrentLife(servant.life);
                    card.setReady(false);
                }
                return Task.CompletedTask;
            });
            return true;
        }
        public class SummonEventArg : EventArg
        {
            public THHPlayer player;
            public Pile from;
            public Card card;
            public int position;
        }
    }
    public static class THHCard
    {
        public static int getCost(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.cost));
        }
        public static int getAttack(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.attack));
        }
        public static int getCurrentLife(this Card card)
        {
            return card.getProp<int>("currentLife");
        }
        public static void setCurrentLife(this Card card, int value)
        {
            card.setProp("currentLife", value);
        }
        public static bool isReady(this Card card)
        {
            return card.getProp<bool>("isReady");
        }
        public static void setReady(this Card card, bool value)
        {
            card.setProp("isReady", value);
        }
        public static int getAttackTimes(this Card card)
        {
            return card.getProp<int>("attackTimes");
        }
        public static void setAttackTimes(this Card card, int value)
        {
            card.setProp("attackTimes", value);
        }
        public static int getMaxAttackTimes(this Card card)
        {
            return 1;
        }
        public static async Task<bool> tryAttack(this Card card, THHGame game, Card target)
        {
            if (card.getAttackTimes() >= card.getMaxAttackTimes())
                return false;
            await game.triggers.doEvent(new AttackEventArg() { card = card, target = target }, async arg =>
            {
                game.logger.log(arg.card + "攻击" + arg.target);
                arg.card.setAttackTimes(arg.card.getAttackTimes() + 1);
                if (arg.card.getAttack() > 0)
                    await arg.target.damage(game, arg.card.getAttack());
                if (arg.target.getAttack() > 0)
                    await arg.card.damage(game, arg.target.getAttack());
                await game.updateDeath();
            });
            return true;
        }
        public class AttackEventArg : EventArg
        {
            public Card card;
            public Card target;
        }
        public static Task damage(this Card card, THHGame game, int value)
        {
            return damage(new Card[] { card }, game, value);
        }
        public static async Task damage(this Card[] cards, THHGame game, int value)
        {
            await game.triggers.doEvent(new DamageEventArg() { cards = cards, value = value }, arg =>
            {
                foreach (Card card in arg.cards)
                {
                    card.setCurrentLife(card.getCurrentLife() - arg.value);
                    game.logger.log(card + "受到" + arg.value + "点伤害，生命值=>" + card.getCurrentLife());
                }
                return Task.CompletedTask;
            });
        }
        public class DamageEventArg : EventArg
        {
            public Card[] cards;
            public int value;
        }
        public static async Task die(this Card[] cards, THHGame game, THHPlayer[] players)
        {
            List<THHPlayer> remainPlayerList = new List<THHPlayer>(game.players);
            await game.triggers.doEvent(new DeathEventArg() { cards = cards }, arg =>
            {
                for (int i = 0; i < arg.cards.Length; i++)
                {
                    Card card = arg.cards[i];
                    if (!players.Any(p => p.field.Contains(card) || p.master == card))//已经不在战场上了，没法死
                        continue;
                    THHPlayer player = players.FirstOrDefault(p => p.master == card);
                    if (player != null)
                    {
                        remainPlayerList.Remove(player);
                        game.logger.log(player + "失败");
                    }
                    else
                    {
                        players[i].field.moveTo(card, players[i].grave);
                        game.logger.log(card + "阵亡");
                    }
                }
                return Task.CompletedTask;
            });
            if (remainPlayerList.Count != game.players.Length)
            {
                if (remainPlayerList.Count > 0)
                    await game.gameEnd(remainPlayerList.ToArray());
                else
                    await game.gameEnd(new THHPlayer[0]);
            }
        }
        public class DeathEventArg : EventArg
        {
            public Card[] cards;
        }
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