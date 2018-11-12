using System;

namespace TouhouHeartstone
{
    [Serializable]
    class Crystal
    {
        public CrystalState state { get; set; }
        public Crystal()
        {
            state = CrystalState.normal;
        }
        public Crystal(CrystalState state)
        {
            this.state = state;
        }
    }
}