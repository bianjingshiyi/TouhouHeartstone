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
        /// <param name="frontend">实现接口的前端对象，Game会通过这个对象与前端进行沟通。</param>
        /// <param name="deck">玩家使用的卡组，数组的第一个整数代表玩家使用的角色卡，后30个是玩家使用的卡组。</param>
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
                new Pile("Skill",new Card(engine,engine.rule.pool[(engine.rule.pool[deck[0]] as MasterCardDefine).skillID])),
                new Pile("Field"),
                new Pile("Grave")
            });
            engine.addPlayer(player);
            dicPlayerFrontend.Add(player, frontend);
        }
        /// <summary>
        /// 游戏初始化
        /// </summary>
        public void init()
        {
            engine.doEvent(new InitEvent());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="cardsRID"></param>
        public void initReplace(int playerIndex, int[] cardsRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            engine.doEvent(new InitReplaceEvent(player, cardsRID.Select(id => { return player["Init"].First(c => { return c.getRID() == id; }); }).ToArray()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="cardRID"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetCardRID"></param>
        public void use(int playerIndex, int cardRID, int targetPosition, int targetCardRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            if (engine.getProp<Player>("currentPlayer") != player)
            {
                EventWitness witness = new UseWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.use_NotYourTurn);
                sendWitness(witness);
                return;
            }
            Card card = player["Hand"].First(c => { return c.getRID() == cardRID; });
            if (player.getProp<int>("gem") < (card.define as ICost).cost)
            {
                EventWitness witness = new UseWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.use_NoEnoughGem);
                sendWitness(witness);
                return;
            }
            Card targetCard = targetCardRID > -1 ? engine.getCards().First(c => { return c.getRID() == targetCardRID; }) : null;
            engine.doEvent(new UseEvent(player, card, targetPosition, targetCard));
        }
        public void attack(int playerIndex, int cardRID, int targetCardRID)
        {
            UberDebug.LogChannel("Core", "攻击事件：玩家" + playerIndex + "使用卡片" + cardRID + "攻击卡片" + targetCardRID);
            Player player = engine.getPlayerAt(playerIndex);
            if (engine.getProp<Player>("currentPlayer") != player)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_NotYourTurn);
                sendWitness(witness);
                return;
            }
            Card card = engine.getCards().First(c => { return c.getRID() == cardRID; });
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
            Card targetCard = engine.getCards().First(c => { return c.getRID() == targetCardRID; });
            if (targetCard.pile.owner["Field"].Any(c => { return c.getProp<bool>("taunt"); }) && targetCard.getProp<bool>("taunt") == false)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_AttackTauntFirst);
                sendWitness(witness);
                return;
            }
            engine.doEvent(new AttackEvent(player, card, targetCard));
        }
        public void turnEnd(int playerIndex)
        {
            Player player = engine.getPlayerAt(playerIndex);
            engine.turnEnd(player);
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
        Dictionary<Player, IFrontend> dicPlayerFrontend { get; } = new Dictionary<Player, IFrontend>();
    }
}