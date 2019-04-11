namespace TouhouHeartstone.Frontend.Model.Witness
{
    public abstract class WitnessHandler
    {
        public abstract string Name { get; }
        public abstract bool HandleWitness(EventWitness witness, DeckController deck);
    }
}
