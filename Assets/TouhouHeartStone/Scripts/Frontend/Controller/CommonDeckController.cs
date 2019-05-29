using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;
using System;
using TouhouHeartstone.Frontend.View;

namespace TouhouHeartstone.Frontend.Controller
{
    /// <summary>
    /// 桌面的公共部分管理器
    /// </summary>
    public class CommonDeckController : MonoBehaviour
    {
        [SerializeField]
        WeatherViewModel weather;

        [SerializeField]
        RoundEndViewModel roundEnd;

        [SerializeField]
        AnimatorPlayer roundStart;

        private void Start()
        {
            roundEnd.RoundEndEvent += OnRoundend;
            TimeRemain = 1;
        }

        public float TimeRemain
        {
            get { return roundEnd.TimeRemain; }
            set { roundEnd.TimeRemain = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// 回合结束按钮点击事件
        /// </summary>
        public event Action OnRoundendBtnClick;

        private void OnRoundend()
        {
            OnRoundendBtnClick?.Invoke();
        }

        /// <summary>
        /// 回合开始后调用这玩意
        /// </summary>
        /// <param name="isMyTurn"></param>
        public void RoundStart(bool isMyTurn)
        {
            roundEnd.Interactivable = isMyTurn;
            TimeRemain = 1;
            counter = false;

            if (isMyTurn)
            {
                roundStart?.Play();
            }
        }

        public void RoundEnd()
        {
            roundEnd.Interactivable = false;
        }

        bool counter = false;
        public void RoundTimerStart()
        {
            counter = true;
        }

        private void Update()
        {
            if (counter)
            {
                TimeRemain -= Time.deltaTime / 10;
                if (TimeRemain == 0)
                    counter = false;
            }
        }
    }
}
