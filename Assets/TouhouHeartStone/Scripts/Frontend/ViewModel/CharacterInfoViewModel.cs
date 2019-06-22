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
    public class CharacterInfoViewModel : CardViewModel
    {
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
            if (e.PropertyName == "HP")
                CardSpec.HP = _Character.HP;
            else if (e.PropertyName == "Atk")
                CardSpec.Atk = _Character.Atk;
            NotifyPropertyChange(e.PropertyName);
        }
    }

}
