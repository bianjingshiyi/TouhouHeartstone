using Controllers;
using TouhouCardEngine;

namespace Game
{
    public interface IActionDefineGetter
    {
        ActionDefine getActionDefine(ActionReference actRef);
    }
    public delegate ActionDefine ActionDefineFinder(ActionReference actRef, string name);
    public delegate EventDefine EventTypeInfoFinder(string name);
    public class THHNodeDefiner : INodeDefiner
    {
        public THHNodeDefiner(IActionDefineGetter defineGetter, ActionDefineFinder defineFinder = null)
        {
            this.defineGetter = defineGetter;
            this.defineFinder = defineFinder;
        }
        public void Define(Node node)
        {
            switch (node)
            {
                case ActionNode action:
                    ActionDefine def = null;
                    if (action.defineRef != null)
                    {
                        def = defineGetter.getActionDefine(action.defineRef);
                    }
                    if (def == null && defineFinder != null)
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
                    evt.define = EventTypeInfoLoader.FindEventTypeInfo(evt.eventName);
                    evt.Define();
                    break;
                case PassiveEntryNode passive:
                    passive.Define();
                    break;
            }
        }
        private IActionDefineGetter defineGetter;
        public ActionDefineFinder defineFinder { get; protected set; }
    }
}
