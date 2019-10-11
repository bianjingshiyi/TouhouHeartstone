using System.Collections.Generic;

using TouhouCardEngine;
using TouhouHeartstone;

namespace Tests
{
    class TestFrontend : IFrontend
    {
        public void sendWitness(EventWitness witness)
        {
            witnessList.Add(witness);
        }
        public List<EventWitness> witnessList { get; } = new List<EventWitness>();
    }
}
