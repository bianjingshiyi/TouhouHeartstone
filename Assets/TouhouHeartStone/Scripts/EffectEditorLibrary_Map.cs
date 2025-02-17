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
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_MAP_KEYS = "获取映射的所有键";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_MAP_VALUES = "获取映射的所有值";

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
        public static GraphMap createMap(
            CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] Pair[] elements)
        {
            var map = new GraphMap();
            foreach (var pair in elements)
            {
                object key, value;
                if (pair.key is BuffReference buffRefK)
                    key = game.getBuffDefine(buffRefK);
                else if (pair.key is DefineReference defRefK)
                    key = game.getDefine(defRefK);
                else
                    key = pair.key;
                if (pair.value is BuffReference buffRefV)
                    value = game.getBuffDefine(buffRefV);
                else if (pair.value is DefineReference defRefV)
                    value = game.getDefine(defRefV);
                else
                    value = pair.value;
                map.SetMap(key, value);
            }
            return map;
        }
        [THHActionMethod(BuiltinActionDefineID.SetMap, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_SET_MAP)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MAP)]
        public static GraphMap setMap(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map,
            [ActionNodeParam(EDITOR_PARAM_NAME_KEY)] object key,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (map == null)
                return null;
            if (key == null)
                return map;
            map.SetMap(key, value);
            return map;
        }
        [THHActionMethod(BuiltinActionDefineID.RemoveMap, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_REMOVE_MAP)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MAP)]
        public static GraphMap removeMap(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map,
            [ActionNodeParam(EDITOR_PARAM_NAME_KEY)] object key)
        {
            if (map == null)
                return null;
            if (key == null)
                return map;
            map.RemoveMap(key);
            return map;
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
        [THHActionMethod(BuiltinActionDefineID.GetMapKeys, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_GET_MAP_KEYS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_KEY)]
        public static object[] getMapKeys(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map)
        {
            if (map == null)
                return null;
            return map.GetMapKeys();
        }
        [THHActionMethod(BuiltinActionDefineID.GetMapValues, StringTable.EDITOR_NODE_CATEGORY_MAP, EDITOR_NODE_NAME_GET_MAP_VALUES)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object[] getMapValues(
            [ActionNodeParam(EDITOR_PARAM_NAME_MAP)] GraphMap map)
        {
            if (map == null)
                return null;
            return map.GetMapValues();
        }
    }
}
