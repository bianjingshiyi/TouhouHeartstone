using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    public class GlobalView : MonoBehaviour
    {
        [SerializeField]
        RectTransform deckRect;

        public CardPositionCalculator CardPositionCalculator { get; private set; }

        [SerializeField]
        CardImageResources images;

        [SerializeField]
        CardTextResources texts;

        public CardImageResource GetCardImageResource(int id) { return GetCardImageResource(id.ToString()); }
        public CardTextResource GetCardTextResource(int id) { return GetCardTextResource(id.ToString()); }

        public CardImageResource GetCardImageResource(string id)
        {
            return images.Get(id, "zh-CN");
        }

        public CardTextResource GetCardTextResource(string id)
        {
            return texts.Get(id, "zh-CN");
        }

        private void Awake()
        {
            CardPositionCalculator = new CardPositionCalculator(deckRect.rect.size);
        }
    }

    public class CardPositionCalculator
    {
        Vector2 screenSize;
        public CardPositionCalculator(Vector2 size)
        {
            screenSize = size;
        }

        /// <summary>
        /// 两张手牌之间的距离
        /// </summary>
        float cardHandSpacing => screenSize.y * 0.08f;

        /// <summary>
        /// 两张屏幕中间的牌之间的距离
        /// </summary>
        float cardCenterSpacing => screenSize.y * 0.2f;

        /// <summary>
        /// 手牌的基础高度
        /// </summary>
        float cardHandBaseY => screenSize.y * 0.01f;

        /// <summary>
        /// 卡片预览的高度
        /// </summary>
        float cardFloatBaseY => screenSize.y * 0.10f;

        /// <summary>
        /// 卡片位置。高于此位置则尝试使用；低于此位置则回到手牌
        /// </summary>
        public float CardUseThresold => screenSize.y * 0.2f;

        /// <summary>
        /// 手牌最大宽度
        /// </summary>
        float maxHandWidth => screenSize.y * 0.5f;

        /// <summary>
        /// 卡片高度的一半
        /// </summary>
        float cardHalfHeight => screenSize.y * 0.2f;

        /// <summary>
        /// 获取丢卡位置的坐标
        /// </summary>
        /// <param name="i"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PositionWithRotation GetCardCenter(int i, int count)
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
        public PositionWithRotation GetCardHand(int i, int count)
        {
            if (i >= count)
                throw new ArgumentOutOfRangeException($"argument i({i}) > count({count})");

            // position
            Vector3 basePos = new Vector3(screenSize.x / 2, cardHandBaseY);

            if (count <= 3)
            {
                basePos.x += (i - (count - 1) / 2f) * cardHandSpacing;

                return new PositionWithRotation() { Position = basePos, Rotation = Vector3.zero };
            }
            else
            {
                var width = maxHandWidth + cardHandSpacing * 0.25f * (count - 3);
                var bt = (count - 1);
                var step = width / bt;
                var deg = ((bt / 2f) - i) * 10;

                basePos.x += step * i - width / 2 + cardHalfHeight * Mathf.Sin(deg * Mathf.Deg2Rad);
                basePos.y -= cardHalfHeight * (1 - Mathf.Cos(deg * Mathf.Deg2Rad)) * 3;

                var rot = new Vector3();
                rot.z = deg;
                return new PositionWithRotation() { Position = basePos, Rotation = rot };
            }
        }

        public Vector3 StackPosition => new Vector3(screenSize.x, 0, 0);

        float retinueSpacing => screenSize.y * 0.15f;

        public PositionWithRotation GetRetinuePosition(int i, int count)
        {
            if (i >= count)
                throw new ArgumentOutOfRangeException($"argument i({i}) > count({count})");

            Vector3 center = screenSize / 2;
            center.y *= 0.8f;

            center.x += (i - (count - 1) / 2f) * retinueSpacing;
            return new PositionWithRotation() { Position = center };
        }

    }
    public struct PositionWithRotation
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }
}
