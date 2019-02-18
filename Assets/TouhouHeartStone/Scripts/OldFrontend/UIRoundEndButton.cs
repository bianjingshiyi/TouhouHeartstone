using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace TouhouHeartstone.OldFrontend
{
    public class UIRoundEndButton : UIButton
    {
        [SerializeField]
        Sprite enableSprite;

        [SerializeField]
        Sprite disableSprite;

        /// <summary>
        /// 结束此轮！
        /// </summary>
        public event Action EndRound;

        bool enableState;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (enableState)
            {
                EndRound?.Invoke();
                enableState = false;
                updateButtonState();
            }
        }

        /// <summary>
        /// 设置是否可以点击结束回合的事件。一般都是设置成true吧（
        /// </summary>
        /// <param name="state"></param>
        public void SetState(bool state)
        {
            enableState = state;
            updateButtonState();
        }

        void updateButtonState()
        {
             GetComponent<SpriteRenderer>().sprite = enableState ? enableSprite : disableSprite;
        }
    }
}
