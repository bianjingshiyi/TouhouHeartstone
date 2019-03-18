using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class CardEngine
    {
        public CardEngine(Rule rule, int randomSeed)
        {
            this.rule = rule;
            random = new Random(randomSeed);
            cardManager = new CardManager();
            recordManager = new RecordManager(this);
        }
        public Rule rule { get; }
        public T getVar<T>(string varName)
        {
            if (dicVar.ContainsKey(varName) && dicVar[varName] is T)
                return (T)dicVar[varName];
            return default(T);
        }
        public void setVar<T>(string varName, T value)
        {
            dicVar[varName] = value;
        }
        public object this[string varName]
        {
            get { return dicVar.ContainsKey(varName) ? dicVar[varName] : null; }
            set { dicVar[varName] = value; }
        }
        Dictionary<string, object> dicVar { get; } = new Dictionary<string, object>();
        internal int registerCard(Card card)
        {
            cardList.Add(card);
            return cardList.Count - 1;
        }
        List<Card> cardList { get; } = new List<Card>();
        public Player getPlayerAt(int playerIndex)
        {
            return (0 <= playerIndex && playerIndex < playerList.Count) ? playerList[playerIndex] : null;
        }
        public int getPlayerIndex(Player player)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i] == player)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 获取所有玩家，玩家在数组中的顺序与玩家被添加的顺序相同。
        /// </summary>
        /// <remarks>为什么不用属性是因为每次都会生成一个数组。</remarks>
        public Player[] getPlayers()
        {
            return playerList.ToArray();
        }
        public void addPlayer(Player player)
        {
            playerList.Add(player);
        }
        private List<Player> playerList { get; } = new List<Player>();
        public void doEvent(Event e)
        {
            e.phase = EventPhase.before;
            beginEvent(e);
            e.phase = EventPhase.logic;
            e.execute(this);
            onEvent(currentEvent);
            e.phase = EventPhase.after;
            endEvent();
        }
        public void beginEvent(Event e)
        {
            e.parent = currentEvent;
            currentEvent = e;
            rule.beforeEvent(this, e);
        }
        public void endEvent()
        {
            //进行游戏规则内容中事件结束之后的处理，比如在事件之后发生的效果。
            rule.afterEvent(this, currentEvent);
            if (currentEvent.parent != null)
                currentEvent = currentEvent.parent;
            else
            {
                eventList.Add(currentEvent);
                currentEvent = null;
            }
        }
        public delegate void EventAction(Event @event);
        public event EventAction onEvent;
        Event currentEvent { get; set; } = null;
        List<Event> eventList { get; } = new List<Event>();
        /// <summary>
        /// 随机整数，注意该函数返回的值可能包括最大值与最小值。
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>介于最大值与最小值之间，可能为最大值也可能为最小值</returns>
        public int randomInt(int min, int max)
        {
            return random.Next(min, max + 1);
        }
        /// <summary>
        /// 随机实数，注意该函数返回的值可能包括最小值，但是不包括最大值。
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>介于最大值与最小值之间，不包括最大值</returns>
        public float randomFloat(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
        Random random { get; set; }
        internal CardManager cardManager { get; private set; }
        internal PlayerManager playerManager { get; private set; }
        public RecordManager recordManager { get; set; }
    }
    public delegate void PlayerIndexWitnessEvent(int playerIndex, EventWitness witness);
    public enum EventPhase
    {
        logic = 0,
        before,
        after
    }
}