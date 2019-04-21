using System;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡片动画
    /// </summary>
    public abstract class CardAnimation : ICardAnimation
    {
        GameObject card;

        public GameObject Card => card;

        public virtual void SetGameObject(GameObject card)
        {
            this.card = card;
        }

        public abstract string AnimationName { get; }

        /// <summary>
        /// 播放对应动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public abstract void PlayAnimation(object sender, EventArgs args, GenericAction callback);
    }


    /// <summary>
    /// 卡片位置的参数
    /// </summary>
    public class CardPositionEventArgs : System.EventArgs
    {
        public CardPositionEventArgs() { }
        public CardPositionEventArgs(int count, int id) { GroupCount = count; GroupID = id; }

        /// <summary>
        /// 组内的牌的数量
        /// </summary>
        public int GroupCount;

        /// <summary>
        /// 此卡在组内的Index
        /// </summary>
        public int GroupID;

        /// <summary>
        /// 组的偏移。其用处不大。
        /// </summary>
        public int GroupOffset;
    }
}
