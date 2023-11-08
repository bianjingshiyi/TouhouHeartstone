using System;
using System.Linq;
using TouhouCardEngine;

namespace Controllers
{
    public class NodeDefine
    {
        public ActionDefine actionDefine;
        public EventTriggerTime eventTriggerTime;
        public NodeDefine(ActionDefine actionDef)
        {
            actionDefine = actionDef;
        }
        public NodeDefine(EventTriggerTime triggerTime)
        {
            eventTriggerTime = triggerTime;
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
            if (eventTriggerTime != null)
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
            var eventVariableInfos = eventTriggerTime?.getVariableInfos();
            if (eventVariableInfos != null)
            {
                return eventVariableInfos.Where(v => v != null).Select(v => v.type).ToArray();
            }
            return null;
        }
    }
}
