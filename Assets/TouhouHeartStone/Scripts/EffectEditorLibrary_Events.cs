using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public partial class EffectEditorLibrary
    {
        /// <summary>
        /// 设置/获取事件变量的事件参数主键。
        /// </summary>
        public const string EVENT_VARIABLE_NODE_EVENT_PARAM_KEY = "eventArg";
        /// <summary>
        /// 设置/获取事件变量的变量名称参数主键。
        /// </summary>
        public const string EVENT_VARIABLE_NODE_EVENT_NAME_PARAM_KEY = "varName";
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

        [TranslateMsg("编辑器/事件_集合名称")]
        public const string EDITOR_NODE_EVENT = "事件";
        [TranslateMsg("编辑器/事件_当前事件_节点名称")]
        public const string EDITOR_NODE_EVENT_THIS_EVENT = "当前事件";
        [TranslateMsg("编辑器/事件_阻止事件_节点名称")]
        public const string EDITOR_NODE_EVENT_STOP_EVENT = "阻止事件";
        [TranslateMsg("编辑器/事件_更改攻击目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET = "更改攻击目标";
        [TranslateMsg("编辑器/事件_更改使用目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_USE_TARGET = "更改使用目标";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET = "伤害/更改受伤卡牌";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE = "伤害/更改伤害源";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE = "伤害/更改伤害值";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_HEAL_CHANGE_VALUE = "治疗/更改治疗值";


        #region 事件

        [ObsoletedKeyMethod(BuiltinActionDefineID.ThisEvent, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_THIS_EVENT)]
        [return: ActionNodeParam(EDITOR_NODE_EVENT)]
        public static EventArg thisEvent(EventArg eventArg)
        {
            return eventArg;
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventVariable, EDITOR_NODE_EVENT, StringTable.EDITOR_NODE_NAME_GET_VARIABLE, "GetVariable")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getVariable(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, true)] EventVarName varName)
        {
            if (eventArg == null || varName == null)
                return null;
            return eventArg.getVar(varName);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.SetEventVariable, EDITOR_NODE_EVENT, StringTable.EDITOR_NODE_NAME_SET_VARIABLE, "SetVariable")]
        public static void setVariable(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, true)] EventVarName varName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (eventArg == null || varName == null)
                return;
            eventArg.setVar(varName, value);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.StopEvent, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_STOP_EVENT)]
        public static void stopEvent(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg)
        {
            if (eventArg == null)
                return;
            eventArg.isCanceled = true;
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.ChangeAttackTarget, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET)]
        public static void changeAttackTarget(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card target)
        {
            if (eventArg is THHCard.AttackEventArg attackArg)
            {
                attackArg.target = target;
            }
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.ChangeUseTarget, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_CHANGE_USE_TARGET)]
        public static void changeUseTargets(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Card[] target)
        {
            if (eventArg.getVar<Card[]>(EVENTVAR.TARGETS).Length > 0)
                eventArg.setVar(EVENTVAR.TARGETS, target);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.DamageEvent_ChangeTarget, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET)]
        public static void damageEvent_changeTarget(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card target)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.target = target;
            }
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.DamageEvent_ChangeSource, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE)]
        public static void damageEvent_changeSource(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card source)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.source = source;
            }
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.DamageEvent_ChangeValue, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE)]
        public static void damageEvent_changeValue(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.damageValue = value;
            }
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.HealEvent_ChangeValue, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_HEAL_CHANGE_VALUE)]
        public static void healEvent_changeValue(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (eventArg is THHCard.HealEventArg healArg)
            {
                healArg.value = value;
            }
        }
        #endregion
    }
}
