using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;

namespace Game
{
    partial class EventManager
    {
        #region 公有方法
        /// <summary>
        /// 获取一个<b>内置</b>动作所绑定的所有直接事件引用。（不包括事件的子事件）
        /// </summary>
        /// <param name="defineId">内置动作ID。</param>
        /// <returns></returns>
        public EventReference[] getBuiltinActionBindedEvents(int defineId)
        {
            var dic = getOrLoadNodeBindingDic();
            if (dic.TryGetValue(defineId, out string[] names))
            {
                return names.Select(n => new EventReference(0, n)).ToArray();
            }
            return null;
        }
        /// <summary>
        /// 获取一个<b>内置</b>动作所绑定的所有事件引用。（包括事件的子事件）
        /// </summary>
        /// <param name="defineId">内置动作ID。</param>
        /// <returns></returns>
        public EventReference[] getBuiltinActionBindedEventsRecursive(int defineId)
        {
            List<EventReference> names = new List<EventReference>();
            var eventRefs = getBuiltinActionBindedEvents(defineId);
            foreach (var eventRef in eventRefs)
            {
                if (names.Contains(eventRef))
                    continue;
                names.Add(eventRef);
                getEventReferenceChildrenNonAlloc(eventRef, names);
            }
            return names.ToArray();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 根据自定义动作的节点图内的节点，获取该自定义动作所具有的所有可能的事件引用。
        /// </summary>
        /// <param name="generatedActionDefine">自定义动作。</param>
        /// <param name="list">传入列表。</param>
        private void getGeneratedActionChildrenEventsNonAlloc(GeneratedActionDefine generatedActionDefine, List<EventReference> list)
        {
            foreach (var node in generatedActionDefine.graph.nodes)
            {
                if (node is ActionNode action)
                {
                    var actionRef = action.defineRef;
                    var actionDefine = game.card.getActionDefine(actionRef);
                    if (actionDefine is GeneratedActionDefine gen && gen.type == NodeDefineType.Event) // 自定义事件。
                    {
                        var eventRef = CardManager.getEventReferenceOfAction(gen);
                        if (list.Contains(eventRef))
                            continue;
                        getGeneratedActionChildrenEventsNonAlloc(gen, list);
                    }
                    else if (game.card.isBuiltinCardPool(actionRef.cardPoolId)) // 内置节点事件。
                    {
                        game.card.getActionBindedEventsRecursive(actionRef);
                        var names = getBuiltinActionBindedEventsRecursive(actionRef.defineId);
                        foreach (var name in names)
                        {
                            if (list.Contains(name))
                                continue;
                            list.Add(name);
                        }
                    }
                }
            }
        }
        private Dictionary<int, string[]> getOrLoadNodeBindingDic()
        {
            if (eventNodeBindingDic == null)
            {
                var assemblies = new Assembly[] { typeof(EffectEditorLibrary).Assembly };
                eventNodeBindingDic = loadNodeBindingDic(assemblies);
            }
            return eventNodeBindingDic;
        }
        private Dictionary<int, string[]> loadNodeBindingDic(Assembly[] assemblies)
        {
            Dictionary<int, string[]> bindingDict = new Dictionary<int, string[]>();
            var methods = assemblies.SelectMany(a => a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)));
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<EventNodeAttribute>() is EventNodeAttribute attribute &&
                    method.GetCustomAttribute<ActionNodeMethodAttribute>() is ActionNodeMethodAttribute methodAttribute)
                {
                    bindingDict.Add(methodAttribute.defineId, attribute.types.Select(t => EventHelper.getName(t)).ToArray());
                }
            }
            return bindingDict;
        }
        #endregion

        #region 属性字段
        private Dictionary<int, string[]> eventNodeBindingDic;
        #endregion
    }
}
