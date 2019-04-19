using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 天气的VM
    /// </summary>
    [Binding]
    public class WeatherViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private Sprite _WeatherSprite;

        /// <summary>
        /// 天气的图片
        /// </summary>
        [Binding]
        public Sprite WeatherSprite
        {
            get { return _WeatherSprite; }
            set { _WeatherSprite = value; NotifyPropertyChange("WeatherSprite"); }
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
