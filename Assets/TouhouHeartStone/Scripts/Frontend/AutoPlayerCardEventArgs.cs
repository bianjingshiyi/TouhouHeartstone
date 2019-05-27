namespace TouhouHeartstone.Frontend
{
    class AutoPlayerCardEventArgs : AutoEventArgs, IPlayerEventArgs, ICardEventArgs
    {
        public AutoPlayerCardEventArgs(EventWitness witness) : base(witness)
        {
        }
        public AutoPlayerCardEventArgs(string eventName, int playerIndex, int cardRID, int cardDID) : base(eventName)
        {
            (this as IPlayerEventArgs).PlayerID = playerIndex;
            (this as ICardEventArgs).CardRID = cardRID;
            (this as ICardEventArgs).CardDID = cardDID;
        }
        int IPlayerEventArgs.PlayerID
        {
            get { return getProp<int>("playerIndex"); }
            set { setProp("playerIndex", value); }
        }
        int ICardEventArgs.CardDID
        {
            get { return getProp<int>("cardDID"); }
            set { setProp("cardDID", value); }
        }
        int ICardEventArgs.CardRID
        {
            get { return getProp<int>("cardRID"); }
            set { setProp("cardRID", value); }
        }
    }
}
