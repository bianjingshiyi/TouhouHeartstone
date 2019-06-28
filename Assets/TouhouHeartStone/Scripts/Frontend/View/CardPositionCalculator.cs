using System;
using TouhouHeartstone.Frontend.View.Animation;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    public class CardPositionCalculator : MonoBehaviour
    {
        [SerializeField]
        RectTransform deckRect = null;

        [SerializeField]
        RectTransform selfServantRoot = null;

        [SerializeField]
        RectTransform oppoServantRoot = null;

        [SerializeField]
        RectTransform selfHandCardRoot = null;

        [SerializeField]
        RectTransform oppoHandCardRoot = null;

        [SerializeField]
        RectTransform selfMasterCard = null;

        [SerializeField]
        RectTransform oppoMasterCard = null;

        Vector2 rectSize => deckRect.rect.size;
        Vector2 screenSize
        {
            get
            {
                return new Vector2(Screen.width, Screen.height);
            }
        }

        /// <summary>
        /// 两张手牌之间的距离
        /// </summary>
        float cardHandSpacing => rectSize.y * 0.08f;

        /// <summary>
        /// 两张屏幕中间的牌之间的距离
        /// </summary>
        float cardCenterSpacing => screenSize.y * 0.2f;

        /// <summary>
        /// 手牌的基础高度
        /// </summary>
        float cardHandBaseY => rectSize.y * 0.01f;

        /// <summary>
        /// 卡片预览的高度
        /// </summary>
        float cardFloatBaseY => rectSize.y * 0.10f;

        /// <summary>
        /// 卡片位置。高于此位置则尝试使用；低于此位置则回到手牌
        /// </summary>
        public float CardUseThresold => rectSize.y * 0.2f;

        /// <summary>
        /// 手牌最大宽度
        /// </summary>
        float maxHandWidth => rectSize.y * 0.5f;

        /// <summary>
        /// 卡片高度的一半
        /// </summary>
        float cardHalfHeight => rectSize.y * 0.2f;

        /// <summary>
        /// 获取丢卡位置的坐标（全局）
        /// </summary>
        /// <param name="i"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PositionWithRotation GetThrowCardPosition(int i, int count)
        {
            if (i >= count)
                throw new ArgumentOutOfRangeException($"argument i({i}) > count({count})");

            Vector3 center = screenSize / 2;

            center.x += (i - (count - 1) / 2f) * cardCenterSpacing;
            return new PositionWithRotation() { Position = center };
        }

        /// <summary>
        /// 获取卡片预览位置的坐标
        /// </summary>
        /// <param name="i"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PositionWithRotation GetCardFlow(int i, int count)
        {
            var pos = GetCardHand(i, count).Position;
            pos.y = cardFloatBaseY;
            return new PositionWithRotation() { Position = pos, Rotation = Vector3.zero };
        }

        /// <summary>
        /// 获取手牌的坐标
        /// </summary>
        /// <param name="i"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PositionWithRotation GetCardHand(int i, int count, bool self = true, bool global = false)
        {
            if (i >= count)
                throw new ArgumentOutOfRangeException($"argument i({i}) > count({count})");

            Vector3 basePos = Vector3.zero;
            float factor = 1;
            if (global)
            {
                basePos = self ? selfHandCardRoot.position : oppoHandCardRoot.position;
                factor = screenSize.y / rectSize.y;
            }

            if (count <= 3)
            {
                basePos.x += (i - (count - 1) / 2f) * cardHandSpacing * factor;

                return new PositionWithRotation() { Position = basePos, Rotation = Vector3.zero };
            }
            else
            {
                int sign = self ? 1 : -1;
                var totalWidth = maxHandWidth + cardHandSpacing * 0.25f * (count - 3) * factor;
                var bt = (count - 1);
                var step = totalWidth / bt;
                var deg = ((bt / 2f) - i) * 10 * sign;

                basePos.x += (step * i - totalWidth / 2) + cardHalfHeight * Mathf.Sin(deg * Mathf.Deg2Rad) * sign * factor;
                basePos.y -= cardHalfHeight * (1 - Mathf.Cos(deg * Mathf.Deg2Rad)) * 3 * sign * factor;

                var rot = new Vector3();
                rot.z = deg;
                return new PositionWithRotation() { Position = basePos, Rotation = rot };
            }
        }

        public Vector3 StackPosition => new Vector3(rectSize.x, 0, 0);

        float retinueSpacing => rectSize.y * 0.15f;

        public PositionWithRotation GetPOV(ObjectPositionEventArgs pos, bool global = false)
        {
            if (pos is CardPositionEventArgs)
            {
                return GetPOV(pos as CardPositionEventArgs, global);
            }
            if (pos is SpecialCardPositionEventArgs)
            {
                return GetPOV(pos as SpecialCardPositionEventArgs);
            }
            throw new InvalidCastException();
        }

        public PositionWithRotation GetPOV(CardPositionEventArgs pos, bool global = false)
        {
            return GetServantPosition(pos.GroupID, pos.GroupCount, pos.SelfSide, global);
        }

        public PositionWithRotation GetPOV(SpecialCardPositionEventArgs pos)
        {
            switch (pos.Type)
            {
                case SpecialCardPositionEventArgs.CardType.MasterCard:
                    return new PositionWithRotation(pos.SelfSide ? selfMasterCard.position : oppoMasterCard.position);
            }
            throw new InvalidCastException();
        }

        /// <summary>
        /// 获取随从位置坐标
        /// </summary>
        /// <param name="i"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PositionWithRotation GetServantPosition(int i, int count, bool self = true, bool global = false)
        {
            if (i >= count)
                throw new ArgumentOutOfRangeException($"argument i({i}) > count({count})");

            Vector3 center = Vector3.zero;
            float factor = 1;
            if (global)
            {
                center = self ? selfServantRoot.position : oppoServantRoot.position;
                factor = screenSize.y / rectSize.y;
            }

            // center.y *= 0.8f;
            center.x += (i - (count - 1) / 2f) * retinueSpacing * factor;
            return new PositionWithRotation() { Position = center };
        }

        public void Awake()
        {
            Debug.Log(selfServantRoot.position);
        }
    }
}
