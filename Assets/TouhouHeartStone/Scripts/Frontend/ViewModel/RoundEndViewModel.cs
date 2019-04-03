using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;
using System;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 回合按钮和绳子的VM
    /// 
    /// 这个在倒计时结束的时候不会触发任何事件。所有的事件触发都是通过model方做的
    /// </summary>
    [Binding]
    public class RoundEndViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        /// <summary>
        /// 回合结束按钮点击事件
        /// </summary>
        public event Action RoundEndEvent;

        [Binding]
        public void RoundEndButtonClick()
        {
            RoundEndEvent?.Invoke();
        }

        private float _TimeRemain;

        /// <summary>
        /// 回合剩余时间
        /// </summary>
        [Binding]
        public float TimeRemain
        {
            get { return _TimeRemain; }
            set
            {
                _TimeRemain = value;
                NotifyPropertyChange("TimeRemain");
            }
        }

        /// <summary>
        /// 可否被点击
        /// </summary>
        [Binding]
        public bool Interactivable
        {
            get
            {
                return _interactivable;
            }
            set
            {
                _interactivable = value;
                NotifyPropertyChange("Interactivable");
            }
        }
        private bool _interactivable;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知 Property 改变
        /// </summary>
        void NotifyPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
