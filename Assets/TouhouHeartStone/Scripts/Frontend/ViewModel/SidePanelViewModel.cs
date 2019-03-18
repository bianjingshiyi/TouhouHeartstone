using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend.ViewModel
{
    [Binding]
    public class SidePanelViewModel :MonoBehaviour, INotifyPropertyChanged
    {
        [SerializeField]
        float width;

        bool collapse;

        [Binding]
        public float Width
        {
            get
            {
                return collapse ? 0 : width;
            }
        }

        [Binding]
        public void SwitchCollapse()
        {
            collapse = !collapse;
            NotifyPropertyChange("Width");
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
