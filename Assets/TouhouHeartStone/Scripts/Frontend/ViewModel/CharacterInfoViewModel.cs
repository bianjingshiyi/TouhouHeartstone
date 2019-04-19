using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

using TouhouHeartstone.Frontend.Model;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 角色基础信息VM
    /// 包括角色的数据和外观等。
    /// </summary>
    [Binding]
    public class CharacterInfoViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private Sprite _CharacterImage;

        [Binding]
        public Sprite CharacterImage
        {
            get { return _CharacterImage; }
            set { _CharacterImage = value; NotifyPropertyChange("CharacterImage"); }
        }

        [Binding]
        public int HP => _Character == null ? 0 : _Character.HP;

        [Binding]
        public int Atk => _Character == null ? 0 : _Character.Atk;

        [Binding]
        public int Defence => _Character == null ? 0 : _Character.Defence;

        private CharacterData _Character = null;
        public CharacterData Character
        {
            get { return _Character; }
            set
            {
                if (_Character == null)
                {
                    _Character = value;
                    NotifyPropertyChange("HP");
                    NotifyPropertyChange("Atk");
                    NotifyPropertyChange("Defence");
                }
                else
                {
                    _Character.PropertyChanged -= notifyPropChangeInner;
                }

                _Character = value;
                _Character.PropertyChanged += notifyPropChangeInner;
            }
        }

        void notifyPropChangeInner(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
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
