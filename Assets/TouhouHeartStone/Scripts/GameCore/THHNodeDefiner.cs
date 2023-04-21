using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class THHNodeDefiner : INodeDefiner
    {
        public THHNodeDefiner(ActionDefineFinder defineFinder, EventTypeInfoFinder eventFinder)
        {
            this.defineFinder = defineFinder;
            eventDefineFinder = eventFinder;
        }
        public void Define(Node node)
        {
            switch (node)
            {
                case ActionNode action:
                    action.define = defineFinder?.Invoke(action.defineName);
                    action.Define();
                    break;
                case ActiveEntryNode active:
                    active.Define();
                    break;
                case PassiveEntryNode passive:
                    passive.Define();
                    break;
            }
        }

        public ActionDefineFinder defineFinder { get; protected set; }
        public EventTypeInfoFinder eventDefineFinder { get; protected set; }
    }
}
