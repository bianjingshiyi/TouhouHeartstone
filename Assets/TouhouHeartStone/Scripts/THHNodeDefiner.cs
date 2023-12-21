using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public delegate ActionDefine ActionDefineFinder(ActionReference actRef, string name);
    public delegate EventDefine EventTriggerTimeFinder(EventReference eventRef);
    public class THHNodeDefiner : INodeDefiner
    {
        public THHNodeDefiner(ActionDefineFinder defineFinder, EventTriggerTimeFinder eventFinder)
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
                    {
                        var eventDefine = eventFinder.Invoke(evt.triggerTime.defineRef);
                        evt.setEventDefine(eventDefine, evt.triggerTime.type);
                        evt.Define();
                    }
                    break;
                case EventRuleEntryNode evtRule:
                    {
                        var eventDefine = eventFinder.Invoke(evtRule.triggerTime.defineRef);
                        evtRule.setEventDefine(eventDefine, evtRule.triggerTime.type);
                        evtRule.Define();
                    }
                    break;
                case PassiveEntryNode passive:
                    passive.Define();
                    break;
            }
        }
        public ActionDefineFinder defineFinder { get; protected set; }
        public EventTriggerTimeFinder eventFinder { get; protected set; }
    }
}
