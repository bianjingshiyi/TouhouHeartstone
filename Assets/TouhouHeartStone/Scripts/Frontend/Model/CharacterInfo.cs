using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouHeartstone.Frontend.Model
{
    /// <summary>
    /// 玩家角色数据
    /// </summary>
    public class CharacterData : INotifyPropertyChanged
    {
        private int _HP;
        public int HP
        {
            get { return _HP; }
            set { _HP = value; NotifyPropertyChange("HP"); }
        }

        private int _Atk;
        public int Atk
        {
            get { return _Atk; }
            set { _Atk = value; NotifyPropertyChange("Atk"); }
        }

        private int _Defence;
        public int Defence
        {
            get { return _Defence; }
            set { _Defence = value; NotifyPropertyChange("Defence"); }
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
