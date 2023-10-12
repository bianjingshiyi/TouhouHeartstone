using Controllers;
using TouhouCardEngine;

namespace Game
{
    public delegate ActionDefine ActionDefineFinder(ActionReference actRef, string name);
    public delegate EventDefine EventDefineFinder(long cardPoolId, string name);
    public class THHNodeDefiner : INodeDefiner
    {
        public THHNodeDefiner(ActionDefineFinder defineFinder, EventDefineFinder eventFinder)
        {
            this.defineFinder = defineFinder;
            this.eventFinder = eventFinder;
        }
        public void Define(Node node)
        {
            switch (node)
            {
                case ActionNode action:
                    ActionDefine def = null;
                    if (defineFinder != null)
                    {
                        def = defineFinder.Invoke(action.defineRef, action.defineName);
                    }
                    action.define = def;
                    if (def != null && action.defineRef == null)
                    {
                        action.defineRef = new ActionReference(def.cardPoolId, def.defineId);
                    }
                    action.Define();
                    break;
                case ActiveEntryNode active:
                    active.Define();
                    break;
                case EventEntryNode evt:
                    evt.define = eventFinder?.Invoke(evt.eventCardPoolId, evt.eventName);
                    evt.Define();
                    break;
                case PassiveEntryNode passive:
                    passive.Define();
                    break;
            }
        }
        public ActionDefineFinder defineFinder { get; protected set; }
        public EventDefineFinder eventFinder { get; protected set; }
    }
}
