using System.ComponentModel;
using System;
using UnityEngine;
using UnityWeld.Binding;
using TouhouHeartstone.Frontend.View;

namespace TouhouHeartstone.Frontend.ViewModel
{
    [Binding]
    public class PrivacyProtectorViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private bool _Visable;
        [Binding]
        public bool Visable
        {
            get { return _Visable; }
            set { _Visable = value; NotifyPropertyChange("Visable"); gameObject.SetActive(_Visable); }
        }

        private int _PlayerID;
        [Binding]
        public int PlayerID
        {
            get { return _PlayerID; }
            set { _PlayerID = value; NotifyPropertyChange("PlayerID"); }
        }


        private string _PlayerName;
        [Binding]
        public string PlayerName
        {
            get { return _PlayerName; }
            set { _PlayerName = value; NotifyPropertyChange("PlayerName"); }
        }

        [Binding]
        public void OnConfirmClick()
        {
            Visable = false;
            OnComfirmEvent?.Invoke(PlayerID);
        }

        public event Action<int> OnComfirmEvent;

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
