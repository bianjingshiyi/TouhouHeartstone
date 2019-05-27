using System;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 卡片动画关联的Animation
    /// </summary>
    public class CardAnimationEventArgs : EventArgs
    {
        /// <summary>
        /// 卡片动画名称
        /// </summary>
        public string AnimationName;
        /// <summary>
        /// 卡片动画附带的参数
        /// </summary>
        public EventArgs EventArgs;

        public CardAnimationEventArgs() { }
        public CardAnimationEventArgs(string name) { AnimationName = name; }
        public CardAnimationEventArgs(string name, EventArgs args) : this(name) { EventArgs = args; }
    }
}
