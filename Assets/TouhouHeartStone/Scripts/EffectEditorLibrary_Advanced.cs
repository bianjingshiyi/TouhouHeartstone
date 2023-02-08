using MukioI18n;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game
{

    public static partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点分类名称")]
        private const string EDIOTR_CATEGORY_NAME_ADVANCED = "高级";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDIOTR_NODE_NAME_SET_TEMP_VARIABLE = "设置临时变量";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDIOTR_NODE_NAME_GET_TEMP_VARIABLE = "获取临时变量";
        [TranslateMsg("编辑器-临时变量节点-参数名称")]
        private const string EDITOR_PARAM_NAME_VARIABLE_NAME = "变量名称";
        /// <summary>
        /// 设置临时变量
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        [ActionNodeMethod(EDIOTR_NODE_NAME_SET_TEMP_VARIABLE, EDIOTR_CATEGORY_NAME_ADVANCED)]
        public static void setTempVariable(Scope scope,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, isConst: true)] string variableName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            var curScope = scope;
            var key = "TEMPVAR_" + variableName;
            while (curScope != null)
            {
                if (curScope.localVarDict.ContainsKey(key))
                {
                    curScope.localVarDict[key] = value;
                    return;
                }
                curScope = curScope.parentScope;
            }
            scope.localVarDict[key] = value;
        }
        /// <summary>
        /// 获取临时变量
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        [ActionNodeMethod(EDIOTR_NODE_NAME_GET_TEMP_VARIABLE, EDIOTR_CATEGORY_NAME_ADVANCED)]
        public static object getTempVariable(Scope scope,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, isConst: true)] string variableName)
        {
            var curScope = scope;
            var key = "TEMPVAR_" + variableName;
            while (curScope != null)
            {
                if (curScope.localVarDict.TryGetValue(key, out object value))
                    return value;
                curScope = curScope.parentScope;
            }
            return null;
        }
    }
}
