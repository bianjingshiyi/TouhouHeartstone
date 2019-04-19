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
}
