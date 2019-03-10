namespace TouhouHeartstone.Backend
{
    public struct PlayerData
    {
        public PlayerData(int id, int[] deck)
        {
            this.id = id;
            this.deck = deck;
        }
        public int id { get; }
        public int[] deck { get; }
    }
}