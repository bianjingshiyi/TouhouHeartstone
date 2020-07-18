using System;

using UnityEngine;
using TouhouCardEngine.Interfaces;
using Game;
namespace UI
{
    [Serializable]
    public class UIAnimation
    {
        [SerializeField]
        string _name;
        public string name
        {
            get { return _name; }
        }
#if UNITY_EDITOR
        [SerializeField]
        string _content;
#endif
        public UIAnimation()
        {
            _name = GetType().Name;
#if UNITY_EDITOR
            _content = ToString();
#endif
        }
        public UIAnimation(string name)
        {
            _name = name;
#if UNITY_EDITOR
            _content = ToString();
#endif
        }
#if UNITY_EDITOR
        public void updateDebugContent()
        {
            _content = ToString();
        }
#endif
        /// <summary>
        /// 当播放动画时会在每一帧被调用，返回值为真表示动画结束，动画将被从队列中剔除。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        [Obsolete]
        public virtual bool update(Table table)
        {
            return true;
        }
        /// <summary>
        /// 是否会阻挡下一个动画？
        /// </summary>
        /// <param name="nextAnim">下一个动画</param>
        /// <returns></returns>
        public virtual bool blockAnim(UIAnimation nextAnim)
        {
            return true;
        }
    }
    [Serializable]
    public abstract class TableAnimation : UIAnimation
    {
        public sealed override bool update(Table table)
        {
            throw new NotImplementedException();
        }
        public virtual void init(TableManager table)
        {
        }
        public abstract bool update(TableManager table);
    }
    public abstract class EventAnimation : TableAnimation
    {
        public abstract IEventArg eventArg { get; set; }
    }
    [Serializable]
    public abstract class EventAnimation<T> : EventAnimation where T : IEventArg
    {
        [SerializeField]
        T _eventArg;
        public override IEventArg eventArg
        {
            get { return _eventArg; }
            set { _eventArg = value is T t ? t : default; }
        }
        public T tEventArg
        {
            get { return eventArg is T t ? t : default; }
        }
        public sealed override void init(TableManager table)
        {
            base.init(table);
            init(table, eventArg is T t ? t : default);
        }
        public virtual void init(TableManager table, T eventArg)
        {
        }
        public sealed override bool update(TableManager table)
        {
            return update(table, eventArg is T t ? t : default);
        }
        public abstract bool update(TableManager table, T eventArg);
    }
    [Serializable]
    public abstract class UIAnimation<T> : UIAnimation where T : IEventArg
    {
        public T eventArg { get; protected set; }
        public UIAnimation(T eventArg)
        {
            this.eventArg = eventArg;
        }
    }
}
