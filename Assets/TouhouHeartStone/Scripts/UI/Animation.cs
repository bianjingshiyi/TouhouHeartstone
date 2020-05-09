using System;

using UnityEngine;
using TouhouCardEngine.Interfaces;

namespace UI
{
    [Serializable]
    public abstract class Animation
    {
        [SerializeField]
        string _name;
        public string name
        {
            get { return _name; }
        }
        public Animation()
        {
            _name = GetType().Name;
        }
        public Animation(string name)
        {
            _name = name;
        }
        /// <summary>
        /// 当播放动画时会在每一帧被调用，返回值为真表示动画结束，动画将被从队列中剔除。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public abstract bool update(Table table);
        /// <summary>
        /// 是否会阻挡下一个动画？
        /// </summary>
        /// <param name="nextAnim">下一个动画</param>
        /// <returns></returns>
        public virtual bool blockAnim(Animation nextAnim)
        {
            return true;
        }
    }
    [Serializable]
    abstract class Animation<T> : Animation where T : IEventArg
    {
        public T eventArg { get; protected set; }
        public Animation(T eventArg)
        {
            this.eventArg = eventArg;
        }
    }
}
