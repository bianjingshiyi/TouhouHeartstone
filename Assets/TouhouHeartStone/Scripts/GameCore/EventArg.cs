using System;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class EventArg : IEventArg
    {
        public string[] beforeNames { get; set; }
        public string[] afterNames { get; set; }
        public object[] args { get; set; }
        public bool isCanceled { get; set; }
        public int repeatTime { get; set; }
        public Func<IEventArg, Task> action { get; set; }
    }
}