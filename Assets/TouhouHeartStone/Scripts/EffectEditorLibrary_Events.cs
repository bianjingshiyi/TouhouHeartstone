using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public partial class EffectEditorLibrary
    {
        //  事件处理
        [TranslateMsg("编辑器/事件_事件_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENT = "事件";
        [TranslateMsg("编辑器/事件_事件类型_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENT_TYPE = "事件类型";
        [TranslateMsg("编辑器/事件_事件集合_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENT_RECORDS = "事件集合";
        [TranslateMsg("编辑器/事件_回合数_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT = "回合数";
        [TranslateMsg("编辑器/参数名称")]
        public const string EDITOR_PARAM_NAME_VARIABLE_NAME = "变量名";

        #region 事件
        [TranslateMsg("编辑器/事件_当前事件_节点名称")]
        public const string EDITOR_NODE_EVENT_THIS_EVENT = "当前事件";
        [ActionNodeMethod(EDITOR_NODE_EVENT_THIS_EVENT, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT)]
        public static EventArg thisEvent(EventArg eventArg)
        {
            return eventArg;
        }

        [TranslateMsg("编辑器/事件_获取变量_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_VARIABLE = "获取变量";
        [ActionNodeMethod(EDITOR_NODE_EVENT_GET_VARIABLE, StringTable.EDITOR_NODE_CATEGORY_EVENT, "GetVariable")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getVariable(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, true)] string varName)
        {
            return eventArg.getVar(varName);
        }
        [TranslateMsg("编辑器/事件_设置变量_节点名称")]
        public const string EDITOR_NODE_EVENT_SET_VARIABLE = "设置变量";
        [ActionNodeMethod(EDITOR_NODE_EVENT_SET_VARIABLE, StringTable.EDITOR_NODE_CATEGORY_EVENT, "SetVariable")]
        public static void setVariable(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, true)] string varName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            eventArg.setVar(varName, value);
        }

        [TranslateMsg("编辑器/事件_阻止事件_节点名称")]
        public const string EDITOR_NODE_EVENT_STOP_EVENT = "阻止事件";
        [ActionNodeMethod(EDITOR_NODE_EVENT_STOP_EVENT, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        public static void stopEvent(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg)
        {
            if (eventArg == null)
                return;
            eventArg.isCanceled = true;
        }


        [TranslateMsg("编辑器/事件_更改攻击目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET = "更改攻击目标";
        [ActionNodeMethod(EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        public static void changeAttackTarget(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card target)
        {
            if (eventArg is THHCard.AttackEventArg attackArg)
            {
                attackArg.target = target;
            }
        }
        [TranslateMsg("编辑器/事件_更改使用目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_USE_TARGET = "更改使用目标";
        [ActionNodeMethod(EDITOR_NODE_EVENT_CHANGE_USE_TARGET, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        public static void changeUseTargets(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Card[] target)
        {
            if (eventArg.getVar<Card[]>(EVENTVAR.TARGETS).Length > 0)
                eventArg.setVar(EVENTVAR.TARGETS, target);
        }

        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET = "伤害/更改受伤卡牌";
        [ActionNodeMethod(EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        public static void damageEvent_changeTarget(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card target)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.target = target;
            }
        }
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE = "伤害/更改伤害源";
        [ActionNodeMethod(EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        public static void damageEvent_changeSource(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card source)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.source = source;
            }
        }
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE = "伤害/更改伤害值";
        [ActionNodeMethod(EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE, StringTable.EDITOR_NODE_CATEGORY_EVENT)]
        public static void damageEvent_changeValue(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.damageValue = value;
            }
        }
        #endregion
    }
}
