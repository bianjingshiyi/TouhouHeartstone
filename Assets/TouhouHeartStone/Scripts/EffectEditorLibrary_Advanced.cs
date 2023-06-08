using MukioI18n;
using TouhouCardEngine;

namespace Game
{

    public static partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点分类名称")]
        public const string EDIOTR_CATEGORY_NAME_ADVANCED = "高级";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDIOTR_NODE_NAME_SET_TEMP_VARIABLE = "设置临时变量";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDIOTR_NODE_NAME_GET_TEMP_VARIABLE = "获取临时变量";
        /// <summary>
        /// 设置临时变量
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        [KeyBondMethod("SetTempVar", EDIOTR_CATEGORY_NAME_ADVANCED, EDIOTR_NODE_NAME_SET_TEMP_VARIABLE)]
        public static void setTempVariable(Flow flow,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, isConst: true)] string variableName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            var key = variableName;
            flow.setFlowVariable(key, value);
        }
        /// <summary>
        /// 获取临时变量
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        [KeyBondMethod("GetTempVar", EDIOTR_CATEGORY_NAME_ADVANCED, EDIOTR_NODE_NAME_GET_TEMP_VARIABLE)]
        public static object getTempVariable(Flow flow,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, isConst: true)] string variableName)
        {
            var key = variableName;
            return flow.getFlowVariable(key);
        }
    }
}
