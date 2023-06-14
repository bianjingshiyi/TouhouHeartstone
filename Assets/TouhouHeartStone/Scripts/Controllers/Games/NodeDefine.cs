using System;
using System.Linq;
using TouhouCardEngine;

namespace Controllers
{
    public class NodeDefine
    {
        public ActionDefine actionDefine;
        public EventDefine eventDefine;
        public NodeDefine(ActionDefine actionDef)
        {
            actionDefine = actionDef;
        }
        public NodeDefine(EventDefine eventDef)
        {
            eventDefine = eventDef;
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
            if (eventDefine != null && eventDefine.variableInfos != null)
            {
                return eventDefine.variableInfos.Where(v => v != null).Select(v => v.type).ToArray();
            }
            return null;
        }
    }
}
