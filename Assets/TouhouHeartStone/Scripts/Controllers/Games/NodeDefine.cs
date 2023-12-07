using System;
using System.Linq;
using TouhouCardEngine;

namespace Controllers
{
    public class NodeDefine
    {
        public ActionDefine actionDefine;
        public EventDefine eventDefine;
        public EventTriggerTimeType eventTriggerType;
        public NodeDefine(ActionDefine actionDef)
        {
            actionDefine = actionDef;
        }
        public NodeDefine(EventDefine eventDef, EventTriggerTimeType triggerType)
        {
            eventDefine = eventDef;
            eventTriggerType = triggerType;
        }
        public bool isObsolete()
        {
            if (actionDefine != null)
            {
                return actionDefine.obsoleteMsg != null;
            }
            return false;
        }
        public string getCategory()
        {
            if (actionDefine != null)
            {
                return actionDefine.category ?? string.Empty;
            }
            if (eventDefine != null)
            {
                return ActionNodeGraphCtrl.EDITOR_CATEGORY_EVENT_ENTRY_NODE;
            }
            return string.Empty;
        }
        public Type[] getOutputTypes()
        {
            if (actionDefine != null)
            {
                return actionDefine.outputDefines.Select(d => d.type).ToArray();
            }
            var eventVariableInfos = eventDefine?.getVariableInfos(eventTriggerType);
            if (eventVariableInfos != null)
            {
                return eventVariableInfos.Where(v => v != null).Select(v => v.type).ToArray();
            }
            return null;
        }
        public PortDefine[] getInputDefines()
        {
            if (actionDefine != null)
            {
                return actionDefine.inputDefines.ToArray();
            }
            else if (isEventEntry())
            {
                return new PortDefine[] { PassiveEntryNode.triggerConditionPortDefine };
            }
            return null;
        }
        public PortDefine[] getOutputDefines()
        {
            if (actionDefine != null)
            {
                return actionDefine.outputDefines.ToArray();
            }
            else if (isEventEntry())
            {
                var variableInfos = eventDefine.getVariableInfos(eventTriggerType);
                return variableInfos.Select(v => PortDefine.Value(v.type, v.name)).Prepend(PassiveEntryNode.actionPortDefine).ToArray();
            }
            return null;
        }
        public bool isEventEntry()
        {
            return eventDefine != null;
        }
        public EventTriggerTime getEventTriggerTime()
        {
            return new EventTriggerTime(eventDefine, eventTriggerType);
        }
    }
}
