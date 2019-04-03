using System;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡片动画抽象类
    /// </summary>
    public interface ICardAnimation
    {
        /// <summary>
        /// 关联的卡片
        /// </summary>
        /// <param name="card"></param>
        GameObject Card { get; }

        /// <summary>
        /// 动画名称
        /// </summary>
        string AnimationName { get; }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        void PlayAnimation(object sender, EventArgs args, GenericAction callback);
    }


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
    }

    /// <summary>
    /// 参数类型错误
    /// </summary>
    [Serializable]
    public class WrongArumentTypeException : Exception
    {
        public WrongArumentTypeException() { }
        public WrongArumentTypeException(Type expected, Type actual) : base($"错误的数据类型。需要{expected}但提供了{actual}") { }
    }
}
