using System;

namespace TouhouHeartstone
{
    [Serializable]
    public struct SetGameIntWitness : IWitness
    {
        public int number { get; set; }
        public string name { get; }
        public int value { get; }
        public SetGameIntWitness(string name, int value)
        {
            number = 0;
            this.name = name;
            this.value = value;
        }
    }
}