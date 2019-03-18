using System;
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
        public void initReplace(int playerIndex, int[] cardIndex)
        {
            engine.doEvent(new InitReplaceEvent(engine.getPlayerAt(playerIndex), cardIndex));
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
}