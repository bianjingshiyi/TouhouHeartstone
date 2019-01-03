using System;

namespace TouhouHeartstone
{
    [Serializable]
    public struct CreateCardsWitness : IWitness
    {
        public int number { get; set; }
        public int player { get; }
        public string pile { get; }
        public CardInstance[] cards { get; }
        public int position { get; }
        public CreateCardsWitness(int player, string pile, CardInstance[] cards, int position)
        {
            number = 0;
            this.player = player;
            this.pile = pile;
            this.cards = cards;
            this.position = position;
        }
    }
}