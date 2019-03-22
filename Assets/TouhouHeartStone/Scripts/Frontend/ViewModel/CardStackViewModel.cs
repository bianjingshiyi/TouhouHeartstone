using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 牌堆的VM
    /// </summary>
    [Binding]
    public class CardStackViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private int _CardCount;
        /// <summary>
        /// 卡片数目
        /// </summary>
        [Binding]
        public int CardCount
        {
            get { return _CardCount; }
            set { _CardCount = value; NotifyPropertyChange("CardCount"); }
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
}
