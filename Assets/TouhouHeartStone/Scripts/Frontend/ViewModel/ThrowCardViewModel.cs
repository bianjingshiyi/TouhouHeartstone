using System.ComponentModel;
using System;
using UnityEngine;
using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 弃牌的VM
    /// </summary>
    [Binding]
    public class ThrowCardViewModel : MonoBehaviour, INotifyPropertyChanged
    {

        private int _Min;
        public int Min
        {
            get { return _Min; }
            set { _Min = value; NotifyPropertyChange("Throwable"); }
        }

        private int _Max;
        public int Max
        {
            get { return _Max; }
            set { _Max = value; NotifyPropertyChange("Throwable"); }
        }

        private int _Current;
        public int Current
        {
            get { return _Current; }
            set { _Current = value; NotifyPropertyChange("Throwable"); }
        }

        [Binding]
        public bool Throwable
        {
            get
            {
                if (Min > 0 && Current < Min) return false;
                if (Max > 0 && Current > Max) return false;
                return true;
            }
        }

        public event Action OnThrow;

        [Binding]
        public void Throw()
        {
            OnThrow?.Invoke();
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
