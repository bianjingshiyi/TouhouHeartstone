using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;

namespace Game
{

    public static partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CREATE_MAP = "创建映射集合";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_SET_MAP = "设置映射";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_REMOVE_MAP = "移除映射";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_HAS_MAP = "是否存在映射";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_MAP_VALUE = "获取映射值";

        [TranslateMsg("编辑器-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_KEY_VALUE_PAIRS = "键值对";
        [TranslateMsg("编辑器-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_MAP = "映射集合";
        [TranslateMsg("编辑器-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_KEY = "键";


        /// <summary>
        /// 创建集合
        /// </summary>
        /// <param name="elements">元素列表</param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.CreateMapCollection, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_CREATE_MAP)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MAP)]
        public static GraphMap createMap()
        {
            var map = new GraphMap();
            return map;
        }
        [THHActionMethod(BuiltinActionDefineID.SetMap, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_SET_MAP)]
        public static void setMap(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map,
            [ActionNodeParam(EDITOR_PARAM_NAME_KEY)] object key,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (map == null)
                return;
            if (key == null)
                return;
            map.SetMap(key, value);
        }
        [THHActionMethod(BuiltinActionDefineID.RemoveMap, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_REMOVE_MAP)]
        public static void removeMap(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map,
            [ActionNodeParam(EDITOR_PARAM_NAME_KEY)] object key)
        {
            if (map == null)
                return;
            if (key == null)
                return;
            map.RemoveMap(key);
        }
        [THHActionMethod(BuiltinActionDefineID.HasMap, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_HAS_MAP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool hasMap(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map,
            [ActionNodeParam(EDITOR_PARAM_NAME_KEY)] object key)
        {
            if (map == null)
                return false;
            if (key == null)
                return false;
            return map.HasMap(key);
        }
        [THHActionMethod(BuiltinActionDefineID.GetMapValue, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_GET_MAP_VALUE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getMapValue(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map,
            [ActionNodeParam(EDITOR_PARAM_NAME_KEY)] object key)
        {
            if (map == null)
                return null;
            if (key == null)
                return null;
            return map.GetMapValue(key);
        }
    }
}
