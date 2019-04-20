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
            engine.afterEvent += afterEvent;
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
        public void initReplace(int playerIndex, int[] cardsRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            engine.doEvent(new InitReplaceEvent(player, cardsRID.Select(id => { return player["Init"].First(c => { return c.getRID() == id; }); }).ToArray()));
        }
        public void use(int playerIndex, int cardRID, int targetPosition, int targetCardRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            Card card = player["Hand"].First(c => { return c.getRID() == cardRID; });
            Card targetCard = targetCardRID > -1 ? engine.getCards().First(c => { return c.getRID() == targetCardRID; }) : null;
            engine.doEvent(new UseEvent(player, card, targetPosition, targetCard));
        }
        public void attack(int playerIndex, int cardRID, int targetCardRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            Card card = engine.getCards().First(c => { return c.getRID() == cardRID; });
            Card targetCard = engine.getCards().First(c => { return c.getRID() == targetCardRID; });
            engine.doEvent(new AttackEvent(player, card, targetCard));
        }
        public void turnEnd(int playerIndex)
        {
            Player player = engine.getPlayerAt(playerIndex);
            engine.doEvent(new TurnEndEvent(player));
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
        Dictionary<Player, IFrontend> dicPlayerFrontend { get; } = new Dictionary<Player, IFrontend>();
    }
}