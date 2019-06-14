using System;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    public class CardPositionCalculator : MonoBehaviour
    {
        [SerializeField]
        RectTransform deckRect = null;

        [SerializeField]
        Transform selfServantRoot = null;

        [SerializeField]
        Transform oppoServantRoot = null;

        [SerializeField]
        Transform selfHandCardRoot = null;

        [SerializeField]
        Transform oppoHandCardRoot = null;

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
        float cardCenterSpacing => rectSize.y * 0.2f;

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
            if (global)
            {
                basePos = self ? selfHandCardRoot.position : oppoHandCardRoot.position;
            }

            if (count <= 3)
            {
                basePos.x += (i - (count - 1) / 2f) * cardHandSpacing;

                return new PositionWithRotation() { Position = basePos, Rotation = Vector3.zero };
            }
            else
            {
                int sign = self ? 1 : -1;
                var totalWidth = maxHandWidth + cardHandSpacing * 0.25f * (count - 3);
                var bt = (count - 1);
                var step = totalWidth / bt;
                var deg = ((bt / 2f) - i) * 10 * sign;

                basePos.x += (step * i - totalWidth / 2) + cardHalfHeight * Mathf.Sin(deg * Mathf.Deg2Rad) * sign;
                basePos.y -= cardHalfHeight * (1 - Mathf.Cos(deg * Mathf.Deg2Rad)) * 3 * sign;

                var rot = new Vector3();
                rot.z = deg;
                return new PositionWithRotation() { Position = basePos, Rotation = rot };
            }
        }

        public Vector3 StackPosition => new Vector3(rectSize.x, 0, 0);

        float retinueSpacing => rectSize.y * 0.15f;

        /// <summary>
        /// 获取随从位置坐标
        /// </summary>
        /// <param name="i"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PositionWithRotation GetRetinuePosition(int i, int count, bool self = true, bool global = false)
        {
            if (i >= count)
                throw new ArgumentOutOfRangeException($"argument i({i}) > count({count})");

            Vector3 center = Vector3.zero;
            if (global)
                center = self ? selfServantRoot.position : oppoServantRoot.position;

            center.y *= 0.8f;
            center.x += (i - (count - 1) / 2f) * retinueSpacing;
            return new PositionWithRotation() { Position = center };
        }
    }
}
