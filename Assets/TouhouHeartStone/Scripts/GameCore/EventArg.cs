using System;
using System.Collections.Generic;
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
        public List<IEventArg> childEventList { get; } = new List<IEventArg>();
        public void addChildEvent(IEventArg eventArg)
        {
            childEventList.Add(eventArg);
        }
        public IEventArg[] getChildEvents()
        {
            return childEventList.ToArray();
        }
        public IEventArg[] children
        {
            get { return childEventList.ToArray(); }
        }
    }
}