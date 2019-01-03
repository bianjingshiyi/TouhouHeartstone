using System;

namespace TouhouHeartstone
{
    [Serializable]
    public struct DestroyCardsWitness : IWitness
    {
        public int number { get; set; }
        public int player { get; }
        public string pile { get; }
        public CardInstance[] cards { get; }
        public DestroyCardsWitness(int player, string pile, CardInstance[] cards)
        {
            number = 0;
            this.player = player;
            this.pile = pile;
            this.cards = cards;
        }
    }
}