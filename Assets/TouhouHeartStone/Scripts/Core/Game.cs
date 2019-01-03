using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class Game
    {
        public Game(Rule rule, int randomSeed)
        {
            this.rule = rule;
            random = new Random(randomSeed);
            cards = new CardManager();
            records = new RecordManager(this);
        }
        public Rule rule { get; }
        public int getPlayerAt(int playerIndex)
        {
            return (0 <= playerIndex && playerIndex < players.count) ? players[playerIndex].id : 0;
        }
        public int getPlayerIndex(int playerId)
        {
            for (int i = 0; i < players.count; i++)
            {
                if (players[i].id == playerId)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 开始游戏，需要提供游戏中玩家的id。
        /// </summary>
        /// <param name="playersID">玩家id数组</param>
        public void start(int[] playersID)
        {
            Event @event = new Event("Init");
            @event["playersID"] = playersID;
            doEvent(@event, (g, e) =>
            {
                //初始化玩家
                playersID = e.getVar<int[]>("playersID");
                if (playersID == null || playersID.Length == 0)
                    return;
                players = new PlayerManager(playersID);
                for (int i = 0; i < players.count; i++)
                {
                    dicWitnessList.Add(players[i], new List<EventWitness>());
                }
            });
        }
        public void doEvent(Event e, Action<Game, Event> logic)
        {
            beginEvent(e);
            logic(this, e);
            endEvent();
        }
        public void beginEvent(Event e)
        {
            e.parent = currentEvent;
            currentEvent = e;
            for (int i = 0; i < players.count; i++)
            {
                EventWitness witness = new EventWitness(e.name);
                witness.parent = dicCurrentWitness.ContainsKey(players[i]) ? dicCurrentWitness[players[i]] : null;
                dicCurrentWitness[players[i]] = witness;
            }
            rule.beforeEvent(this, e);
        }
        public void endEvent(params Player[] visiblePlayers)
        {
            rule.afterEvent(this, currentEvent);
            if (visiblePlayers != null && visiblePlayers.Length > 0)
            {
                for (int i = 0; i < players.count; i++)
                {
                    if (visiblePlayers.Contains(players[i]))
                    {
                        //特定玩家可见
                        currentEvent.copyVars(dicCurrentWitness[players[i]]);
                        if (dicCurrentWitness[players[i]].parent != null)
                            dicCurrentWitness[players[i]] = dicCurrentWitness[players[i]].parent;
                        else
                            sendWitness(players[i]);
                    }
                    else
                    {
                        //特定玩家不可见
                        EventWitness witness = dicCurrentWitness[players[i]];
                        if (witness.parent != null)
                        {
                            dicCurrentWitness[players[i]] = witness.parent;
                            witness.parent = null;
                        }
                        else
                            dicCurrentWitness[players[i]] = null;
                    }
                }
            }
            else
            {
                //所有玩家可见
                for (int i = 0; i < players.count; i++)
                {
                    currentEvent.copyVars(dicCurrentWitness[players[i]]);
                    if (dicCurrentWitness[players[i]].parent != null)
                        dicCurrentWitness[players[i]] = dicCurrentWitness[players[i]].parent;
                    else
                        sendWitness(players[i]);
                }
            }
            if (currentEvent.parent != null)
            {
                currentEvent = currentEvent.parent;
            }
            else
            {
                eventList.Add(currentEvent);
                currentEvent = null;
            }
        }
        void sendWitness(Player player)
        {
            dicWitnessList[player].Add(dicCurrentWitness[player]);
            dicCurrentWitness[player] = null;
        }
        Dictionary<Player, EventWitness> dicCurrentWitness { get; } = new Dictionary<Player, EventWitness>();
        Dictionary<Player, List<EventWitness>> dicWitnessList { get; } = new Dictionary<Player, List<EventWitness>>();
        Event currentEvent { get; set; } = null;
        List<Event> eventList { get; } = new List<Event>();
        public void initReplace(int playerId, CardInstance[] cards)
        {
            if (cards.Length > 0)
                records.addRecord(new InitReplaceRecord(playerId, cards));
            preparedPlayers.Add(playerId);
            //if (players.orderedPlayers.All(e => { return preparedPlayers.Contains(e.id); }))
            duelStart();
        }
        HashSet<int> preparedPlayers { get; } = new HashSet<int>();
        void duelStart()
        {
            records.addRecord(new DuelStartRecord());
            turnStart(players.orderedPlayers[0]);
        }
        void turnStart(Player player)
        {
            records.addRecord(new TurnStartRecord(player.id));
            records.addRecord(new AddCrystalRecord(player.id, 1, CrystalState.normal));
            draw(player, 1);
        }
        void draw(Player player, int count)
        {
            records.addRecord(new DrawRecord(player.id, 1));
        }
        public void use(int playerId, int instance, int position, int target)
        {
            records.addRecord(new UseRecord(playerId, new CardInstance(instance, 0), position, new CardInstance(target, 0)));
        }
        public void turnEnd(int playerId)
        {
            Player player = players.getPlayer(playerId);
            int index = Array.IndexOf(players.orderedPlayers, player) + 1;
            if (index >= players.orderedPlayers.Length)
                index = 0;
            Player nextPlayer = players.orderedPlayers[index];
            turnStart(nextPlayer);
        }
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
        public Player[] getPlayers()
        {
            return players.ToArray();
        }
        internal CardManager cards { get; private set; }
        internal PlayerManager players { get; private set; }
        public RecordManager records { get; set; }
    }
}