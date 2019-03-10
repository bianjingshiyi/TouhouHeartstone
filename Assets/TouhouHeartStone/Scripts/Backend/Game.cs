using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone.Backend
{
    public class Game
    {
        public Game()
        {
            engine = new CardEngine(new HeartStoneRule(), (int)DateTime.Now.ToBinary());
            engine.onEvent += onEvent;
        }
        CardEngine engine { get; }
        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="deck"></param>
        public void addPlayer(IFrontend frontend, int[] deck)
        {
            Card[] cards = new Card[deck.Length - 1];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = new Card(engine, engine.rule.pool[deck[i + 1]]);
            }
            Player player = new Player(frontend.id, new Pile[]
            {
                new Pile("Deck",cards),
                new Pile("Init"),
                new Pile("Hand"),
                new Pile("Master",new Card(engine,engine.rule.pool[deck[0]])),
                new Pile("Field"),
                new Pile("Grave")
            });
            engine.addPlayer(player);
            dicPlayerFrontend.Add(player, frontend);
        }
        public void init()
        {
            engine.doEvent(new InitEvent());
        }
        private void onEvent(Event @event)
        {
            if (@event is VisibleEvent)
            {
                foreach (Player player in engine.getPlayers())
                {
                    dicPlayerFrontend[player].sendWitness((@event as VisibleEvent).getWitness(engine, player));
                }
            }
        }
        Dictionary<Player, IFrontend> dicPlayerFrontend { get; } = new Dictionary<Player, IFrontend>();
    }
    class InitEvent : VisibleEvent
    {
        public InitEvent() : base("onInit")
        {
        }
        public override void execute(CardEngine core)
        {
            //决定玩家行动顺序
            List<Player> remainedList = new List<Player>(core.getPlayers());
            Player[] sortedPlayers = new Player[remainedList.Count];
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                int index = core.randomInt(0, remainedList.Count - 1);
                sortedPlayers[i] = remainedList[index];
                remainedList.RemoveAt(index);
            }
            core.setVar("sortedPlayers", sortedPlayers);
            //抽初始卡牌
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                int count = i == 0 ? 3 : 4;
                sortedPlayers[i]["Deck"].moveCardTo(sortedPlayers[i]["Deck"][sortedPlayers[i]["Deck"].count - count, sortedPlayers[i]["Deck"].count - 1], sortedPlayers[i]["Init"], 0);
            }
        }
        public override EventWitness getWitness(CardEngine core, Player player)
        {
            EventWitness witness = new EventWitness("onInit");
            //双方玩家所使用的卡组主人公
            witness.setVar("players_master_define_id", core.getPlayers().Select(e => { return e["Master"][0].define.id; }).ToArray());
            //然后是玩家的先后行动顺序
            witness.setVar("sortedPlayers_id", core.getVar<Player[]>("sortedPlayers").Select(e => { return e.id; }).ToArray());
            //接着是初始手牌
            witness.setVar("self_init_define_id", player["Init"].Select(e => { return e.define.id; }).ToArray());
            return witness;
        }
    }
}