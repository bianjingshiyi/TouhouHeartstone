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
            roundEnd.TimeRemain = 1;
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
            roundEnd.TimeRemain = 0.6f;
            if (isMyTurn)
            {
                roundStart?.Play();
            }
        }

        public void RoundEnd()
        {
            roundEnd.Interactivable = false;
        }
    }
}
