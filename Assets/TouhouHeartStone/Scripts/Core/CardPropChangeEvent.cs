using System.Linq;

namespace TouhouHeartstone
{
    public class CardPropChangeEvent : VisibleEvent
    {
        public CardPropChangeEvent(Player[] players, string[] pileNames, Card[] cards, string[] propNames, PropertyChangeType[] changeTypes, object[] values) : base("onCardPropChange")
        {
            this.players = players;
            this.pileNames = pileNames;
            this.cards = cards;
            this.propNames = propNames;
            this.changeTypes = changeTypes;
            this.values = values;
            cardsIndex = new int[players.Length];
        }
        Player[] players { get; }
        string[] pileNames { get; }
        Card[] cards { get; }
        string[] propNames { get; }
        PropertyChangeType[] changeTypes { get; }
        object[] values { get; }
        public override void execute(CardEngine engine)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                cardsIndex[i] = players[i][pileNames[i]].indexOf(cards[i]);
                if (0 <= cardsIndex[i] && cardsIndex[i] < players[i][pileNames[i]].count)
                {
                    if (changeTypes[i] == PropertyChangeType.set)
                        cards[i].dicProp[propNames[i]] = values[i];
                    else
                    {
                        if (values[i] is int)
                            cards[i].dicProp[propNames[i]] = cards[i].getProp<int>(propNames[i]) + (int)values[i];
                        else if (values[i] is float)
                            cards[i].dicProp[propNames[i]] = cards[i].getProp<float>(propNames[i]) + (float)values[i];
                    }
                }
            }
        }
        int[] cardsIndex { get; }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onCardPropChange");
            witness.setVar("playersIndex", players.Select(p => { return engine.getPlayerIndex(p); }).ToArray());
            witness.setVar("pileNames", pileNames);
            witness.setVar("cardsIndex", cardsIndex);
            witness.setVar("propNames", propNames);
            witness.setVar("changeTypes", changeTypes);
            witness.setVar("values", values);
            return witness;
        }
    }
}