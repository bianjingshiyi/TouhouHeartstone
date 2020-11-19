using UnityEngine;
using BJSYGameCore;
using UI;
using TouhouHeartstone;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine.Interfaces;
using System.Reflection;
using TouhouCardEngine;
namespace Game
{
    public class EventAnimManager
    {
        #region 公开成员
        /// <summary>
        /// 为事件设置动画。
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="anim"></param>
        public void setAnim4Event(string eventName, EventAnimation anim)
        {
            if (builtInAnimationList.ContainsKey(eventName))
            {
                UberDebug.LogWarningChannel("UI", "事件存在冲突的动画" + eventName);
                builtInAnimationList[eventName] = anim;
            } else builtInAnimationList.Add(eventName,anim);
        }
        /// <summary>
        /// 当事件发生时，为事件获取一个新的动画实例。
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public EventAnimation getAnim4Event(string eventName)
        {
            if (builtInAnimationList.TryGetValue(eventName, out EventAnimation anim))
                return anim.clone();
            return default;
        }
        public void addAnimToQueue(TableAnimation anim)
        {
            animQueue.AddLast(anim);
        }
        public TableAnimation[] getAnimsInQueue()
        {
            return animQueue.ToArray();
        }
        #endregion
        #region Builtin
        Dictionary<string, EventAnimation> builtInAnimationList { get; } = new Dictionary<string, EventAnimation>();
        LinkedList<EventAnimation> animQueue { get; } = new LinkedList<EventAnimation>();

        Dictionary<Type, ConstructorInfo> animConstructorDic { get; } = new Dictionary<Type, ConstructorInfo>();
        /// <summary>
        /// 加载动画
        /// </summary>
        /// <param name="assembly"></param>
        public void loadAnim(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                Type baseType = type.BaseType;
                if (!baseType.IsGenericType)
                    continue;
                if (baseType.GetGenericTypeDefinition() != typeof(UIAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(EventAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(RequestAnimation<>))
                    continue;
                Type paraType = baseType.GetGenericArguments()[0];
                if (!paraType.IsSubclassOf(typeof(EventArg)) &&
                    !paraType.IsSubclassOf(typeof(Request)))
                    continue;
                setAnim(paraType, type);
            }
        }
        /// <summary>
        /// 设置动画
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="animType"></param>
        public void setAnim(Type eventType, Type animType)
        {
            foreach (ConstructorInfo constructor in animType.GetConstructors())
            {
                var args = constructor.GetParameters();
                if (args.Length < 1 || (args.Length == 1 && args[0].ParameterType == eventType))
                {
                    if (animConstructorDic.ContainsKey(eventType))
                    {
                        UberDebug.LogWarningChannel("UI", "存在冲突的动画" + animType);
                        break;
                    }
                    animConstructorDic.Add(eventType, constructor);
                    break;
                }
            }
        }
        public void setEventAnim<TEvent, TAnim>() where TEvent : IEventArg where TAnim : UIAnimation
        {
            setAnim(typeof(TEvent), typeof(TAnim));
        }
        public void setRequestAnim<TRequest, TAnim>() where TRequest : IRequest where TAnim : UIAnimation
        {
            setAnim(typeof(TRequest), typeof(TAnim));
        }
        /// <summary>
        /// 获取事件对应的动画
        /// </summary>
        /// <param name="eventArg"></param>
        /// <returns></returns>
        public UIAnimation getEventAnim(TableManager table, IEventArg eventArg)
        {
            Type type = eventArg.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                UIAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                    anim = animConstructorDic[type].Invoke(new object[0]) as UIAnimation;
                else if (animConstructorDic[type].GetParameters().Length == 1)
                    anim = animConstructorDic[type].Invoke(new object[] { eventArg }) as UIAnimation;
                if (anim is EventAnimation tAnim)
                {
                    tAnim.eventArg = eventArg;
                    tAnim.init(table);
                }
                return anim;
            }
            else
                return null;
        }
        public RequestAnimation getRequestAnim(IRequest request)
        {
            Type type = request.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                RequestAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                {
                    anim = animConstructorDic[type].Invoke(new object[0]) as RequestAnimation;
                    anim.request = request;
                }
                return anim;
            }
            else
                return null;
        }
        #endregion
        #region Editable
        [Serializable]
        class EditableAnim : TableAnimation
        {
            [TypeString(typeof(EventArg))]
            [SerializeField]
            string _eventType;
            public override bool update(TableManager table, float time)
            {
                throw new NotImplementedException();
            }
        }
        [Header("Animations")]
        [SerializeField]
        List<EditableAnim> _editableAnims = new List<EditableAnim>();
        #endregion
        [SerializeField]
        List<UIAnimation> _animationQueue = new List<UIAnimation>();
        public void addAnim(UIAnimation anim)
        {
            _animationQueue.Add(anim);
        }
        public UIAnimation[] getAnimQueue()
        {
            return _animationQueue.ToArray();
        }
        public void updateAnim(TableManager table, float time)
        {
            if (_animationQueue.Count > 0)
            {
                for (int i = 0; i < _animationQueue.Count; i++)
                {
                    UIAnimation anim = _animationQueue[i];
#if UNITY_EDITOR
                    anim.updateDebugContent();
#endif
                    bool isBlocked = false;
                    if (i == 0)
                    {
                        //第一个永远不被阻挡
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            UIAnimation prevAnim = _animationQueue[j];
                            if (prevAnim.blockAnim(anim))
                            {
                                isBlocked = true;
                                break;
                            }
                        }
                    }
                    if (isBlocked)
                        continue;
                    try
                    {
                        if (anim is TableAnimation tAnim ? tAnim.update(table, time) : anim.update(table.ui))
                        {
                            _animationQueue.RemoveAt(i);
                            i--;
                        }
                    }
                    catch (Exception e)
                    {
                        _animationQueue.RemoveAt(i);
                        i--;
                        Debug.LogError("动画" + anim + "播放异常被跳过：" + e);
                    }
                }
            }
        }
    }
}
