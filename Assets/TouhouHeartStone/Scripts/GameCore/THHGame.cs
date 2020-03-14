using System;
using System.Linq;
using System.Collections.Generic;
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
        public Task init()
        {
            return triggers.doEvent(new InitEventArg(), arg =>
            {
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
                    card.setLife(10);
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
        public Player currentPlayer { get; private set; }
        Task turnStart(THHPlayer player)
        {
            return triggers.doEvent(new TurnStartEventArg() { player = player }, async arg =>
            {
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
        }
        public class TurnStartEventArg : EventArg
        {
            public THHPlayer player;
        }
        public Task turnEnd(THHPlayer player)
        {
            if (currentPlayer != player)
                throw new NotYourTurnException(player);
            return triggers.doEvent(new TurnEndEventArg() { player = player }, arg =>
            {
                currentPlayer = null;
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
            List<Card> deathList = new List<Card>();
            foreach (THHPlayer player in players)
            {
                foreach (Card card in player.field)
                {
                    if (card.getLife() <= 0)
                        deathList.Add(card);
                }
            }
        }
        [Obsolete]
        public void use(int playerIndex, int cardRID, int targetPosition, int[] targetCardsRID)
        {
            throw new NotImplementedException();
        }
        public void attack(int playerIndex, int cardRID, int targetCardRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            if (engine.getProp<Player>("currentPlayer") != player)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_NotYourTurn);
                sendWitness(witness);
                return;
            }
            Card card = engine.getCard(cardRID);
            if (!card.getProp<bool>("isReady"))
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_waitOneTurn);
                sendWitness(witness);
                return;
            }
            if (card.getProp<int>("attackTimes") > 0)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_AlreadyAttacked);
                sendWitness(witness);
                return;
            }
            Card targetCard = engine.getCard(targetCardRID);
            if (targetCard.pile.owner["Field"].Any(c => { return c.getProp<bool>("taunt"); }) && targetCard.getProp<bool>("taunt") == false)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_AttackTauntFirst);
                sendWitness(witness);
                return;
            }
            engine.attack(player, card, targetCard);
        }
        public void turnEnd(int playerIndex)
        {
            throw new NotImplementedException();
        }

        public int getNextPlayerIndex(int playerIndex)
        {
            Player player = engine.getPlayerAt(playerIndex);
            Player[] sortedPlayers = engine.getProp<Player[]>("sortedPlayers");
            int index = Array.IndexOf(sortedPlayers, player);
            index++;
            if (index == sortedPlayers.Length)
                index = 0;
            return engine.getPlayerIndex(sortedPlayers[index]);
        }
        public int getCardDID(int cardRID)
        {
            return engine.getCard(cardRID).define.id;
        }
        public int[] getCardsDID(int[] cardsRID)
        {
            return cardsRID.Select(rid => { return getCardDID(rid); }).ToArray();
        }
        public bool isValidTarget(int cardRID, string effectName, int targetCardRID)
        {
            Card card = engine.getCard(cardRID);
            Card targetCard = engine.getCard(targetCardRID);
            Effect onUseEffect = card.define.effects?.FirstOrDefault(e => { return e.trigger == effectName; });
            return onUseEffect.checkTarget(engine, card.pile.owner, card, targetCard);
        }
        public int[] getTargets(int cardRID, string effectName)
        {
            Card card = engine.getCard(cardRID);
            Effect onUseEffect = card.define.effects?.FirstOrDefault(e => { return e.trigger == effectName; });
            if (onUseEffect != null)
                return engine.getCharacters(c => { return onUseEffect.checkTarget(engine, card.pile.owner, card, c); }).Select(c => { return c.id; }).ToArray();
            else
                return new int[0];
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
        void sendWitness(EventWitness witness)
        {
            foreach (Player player in engine.getPlayers())
            {
                dicPlayerFrontend[player].sendWitness(witness);
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
        public int tired { get; private set; } = 0;
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
                return game.triggers.doEvent(new TiredEventArg() { player = this }, arg =>
                {
                    arg.player.tired++;
                    engine.damage(player["Master"][0], player.getProp<int>("tired"));
                    return Task.CompletedTask;
                });
            }
            return game.triggers.doEvent(new DrawEventArg() { player = this }, arg =>
            {
                arg.card = arg.player.deck.top;
                arg.player.deck.moveTo(arg.card, arg.player.hand, arg.player.hand.count);
                return Task.CompletedTask;
            });
        }
        public class DrawEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
        }
        public class TiredEventArg : EventArg
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
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, targets = targets }, async arg =>
            {
                await arg.player.setGem(game, arg.player.gem - arg.card.getCost());
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
                arg.from.moveTo(arg.card, arg.player.field, arg.position);
                if (card.define is ServantCardDefine servant)
                {
                    card.setLife(servant.life);
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
        public static int getLife(this Card card)
        {
            return card.getProp<int>("currentLife");
        }
        public static void setLife(this Card card, int value)
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
        public static async Task damage(this Card[] cards, THHGame game, int value)
        {
            await game.triggers.doEvent(new DamageEventArg() { cards = cards, value = value }, arg =>
            {
                foreach (Card card in arg.cards)
                {
                    card.setLife(card.getLife() - arg.value);
                }
                return Task.CompletedTask;
            });
            await game.updateDeath();
        }
        public class DamageEventArg : EventArg
        {
            public Card[] cards;
            public int value;
        }
        public static async Task die(this Card[] cards, THHGame game, THHPlayer[] players)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                Card card = cards[i];
                if (!players.Any(p => p.hand.Contains(card)))//已经不在战场上了，没法死
                    continue;
                if (card)
            }

            List<Player> remainPlayerList = new List<Player>(game.players);
            for (int i = 0; i < cards.Length; i++)
            {
                Pile pile = cards[i].pile;
                Player player = pile.owner;
                if (cards[i] != player["Master"][0])
                    pile.moveTo(cards[i], player["Grave"], player["Grave"].count);
                else
                    remainPlayerList.Remove(player);
            }
            if (remainPlayerList.Count < engine.getPlayers().Length)
            {
                if (remainPlayerList.Count > 0)
                    engine.doEvent(new GameEndEvent(remainPlayerList.ToArray()));
                else
                    engine.doEvent(new GameEndEvent(new Player[0]));
            }
        }
        public class DeathEventArg : EventArg
        {

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