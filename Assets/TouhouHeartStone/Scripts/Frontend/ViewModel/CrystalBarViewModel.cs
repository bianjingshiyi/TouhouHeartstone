using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 水晶条的VM
    /// </summary>
    [Binding]
    public class CrystalBarViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private int _CrystalTotal;
        [Binding]
        public int CrystalTotal
        {
            get { return _CrystalTotal; }
            set
            {
                _CrystalTotal = value;
                NotifyPropertyChange("CrystalTotal");
                NotifyPropertyChange("CrystalUseable");
            }
        }

        private int _CrystalUsed;
        [Binding]
        public int CrystalUsed
        {
            get { return _CrystalUsed; }
            set
            {
                _CrystalUsed = value;
                NotifyPropertyChange("CrystalUsed");
                NotifyPropertyChange("CrystalUseable");
            }
        }

        private int _CrystalDisable;
        [Binding]
        public int CrystalDisable
        {
            get { return _CrystalDisable; }
            set
            {
                _CrystalDisable = value;
                NotifyPropertyChange("CrystalDisable");
                NotifyPropertyChange("CrystalUseable");
            }
        }

        private int _CrystalHighlight;
        [Binding]
        public int CrystalHighlight
        {
            get { return _CrystalHighlight; }
            set
            {
                _CrystalHighlight = value;
                NotifyPropertyChange("CrystalHighlight");
            }
        }

        [Binding]
        public int CrystalUseable
        {
            get { return CrystalTotal - CrystalDisable - CrystalUsed; }
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
