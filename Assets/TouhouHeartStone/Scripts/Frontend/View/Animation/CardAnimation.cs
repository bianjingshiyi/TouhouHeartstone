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
        [Obsolete("使用带参数版")]
        public CardPositionEventArgs() { }

        [Obsolete("使用带边的版本")]
        public CardPositionEventArgs(int count, int id)
        {
            GroupCount = count;
            GroupID = id;
            if (id >= count)
                throw new ArgumentOutOfRangeException($"卡片位置{id}越界（总数{count}）");
        }

        public CardPositionEventArgs(int count, int id, bool selfSide)
        {
            GroupCount = count;
            GroupID = id;
            if (id >= count)
                throw new ArgumentOutOfRangeException($"卡片位置{id}越界（总数{count}）");

            SelfSide = selfSide;
        }

        /// <summary>
        /// 组内的牌的数量
        /// </summary>
        public int GroupCount;

        /// <summary>
        /// 此卡在组内的Index
        /// </summary>
        public int GroupID;

        /// <summary>
        /// 卡片所属边
        /// </summary>
        public bool SelfSide;
    }

    public class ServantAttackEventArgs : EventArgs
    {
        public CardPositionEventArgs SelfServant { get; }
        public CardPositionEventArgs TargetServant { get; }

        public ServantAttackEventArgs(CardPositionEventArgs self, CardPositionEventArgs target)
        {
            SelfServant = self;
            TargetServant = target;
        }

        public ServantAttackEventArgs(int selfID, int selfCount, int targetID, int targetCount, bool isSelf) :
            this(new CardPositionEventArgs(selfCount, selfID, isSelf),
                new CardPositionEventArgs(targetCount, targetID, !isSelf))
        { }
    }

    public class IntEventArgs : EventArgs
    {
        public int Value { get; }
        public IntEventArgs(int val) { Value = val; }
    }
}
