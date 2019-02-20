using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend.ViewModel
{
    [Binding]
    public class CrystalItem : MonoBehaviour, INotifyPropertyChanged
    {
        private CrystalState _State;

        [Binding]
        public CrystalState State
        {
            get { return _State; }
            set { _State = value; NotifyPropertyChange("State"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知 Property 改变
        /// </summary>
        void NotifyPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum CrystalState
    {
        /// <summary>
        /// 禁用
        /// </summary>
        hidden,
        /// <summary>
        /// 通常
        /// </summary>
        normal,
        /// <summary>
        /// 已使用
        /// </summary>
        used,
        /// <summary>
        /// 高亮
        /// </summary>
        highlight,
        /// <summary>
        /// 禁用
        /// </summary>
        disable
    }
}
