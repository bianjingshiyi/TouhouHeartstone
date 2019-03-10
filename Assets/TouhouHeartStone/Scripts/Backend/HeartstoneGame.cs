using System;

namespace TouhouHeartstone.Backend
{
    public class HeartstoneGame
    {
        public HeartstoneGame()
        {
            //core = new TouhouHeartstone.Game(new HeartStoneRule(), (int)DateTime.Now.ToBinary());
            //core.onEvent += onEvent;
        }
        private void onEvent(int playerIndex, Event @event)
        {
            throw new NotImplementedException();
        }
        Game core { get; }
        public void init(PlayerData[] playerDatas)
        {
            //Player[] players = new Player[playerDatas.Length];
            //for (int i = 0; i < players.Length; i++)
            //{
            //    Card[] deck = new Card[playerDatas[i].deck.Length];
            //    for (int j = 0; j < deck.Length; j++)
            //    {
            //        deck[i] = new Card(core, core.rule.pool[playerDatas[i].deck[j]]);
            //    }
            //    players[i] = new Player(playerDatas[i].id, new Pile[]
            //    {
            //        new Pile("Deck", deck),
            //        new Pile("Init"),
            //        new Pile("Hand")
            //    });
            //}
            //core.init(players);
        }
        public event PlayerIndexWitnessEvent onWitness;
    }
}