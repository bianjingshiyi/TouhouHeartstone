using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    /// <summary>
    /// 回合倒计时的View
    /// </summary>
    public class RoundTimeoutView : RectTransformView
    {
        [SerializeField]
        float startTime = 30;

        float originalWidth;

        private void Awake()
        {
            originalWidth = Width;
        }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public float RemainTime
        {
            set
            {
                if (value > startTime) Width = originalWidth;
                else Width = originalWidth * (value / startTime);
            }
            get { return startTime; }
        }
    }
}
