using IGensoukyo.Utilities;
using TouhouHeartstone.Frontend.ViewModel;

using UnityEngine;
namespace TouhouHeartstone.Frontend.View
{
    public class CrystalBarView : ItemSpawner<CrystalItem>
    {
        private int _Total;
        public int Total
        {
            get { return _Total; }
            set { _Total = value; onCrystalStateChange(); }
        }

        private int _Used;
        public int Used
        {
            get { return _Used; }
            set { _Used = value; onCrystalStateChange(); }
        }

        private int _Disable;
        public int Disable
        {
            get { return _Disable; }
            set { _Disable = value; onCrystalStateChange(); }
        }

        private int _Highlight;
        public int Highlight
        {
            get { return _Highlight; }
            set { _Highlight = value; onCrystalStateChange(); }
        }

        void onCrystalStateChange()
        {
            while (Total > ItemCount) SpawnItem();

            for (int i = 0; i < ItemCount; i++)
            {
                if(i >= Total)
                {
                    GetItemAt(i).State = ViewModel.CrystalState.hidden;
                    continue;
                }

                if (Disable > i)
                {
                    GetItemAt(i).State = ViewModel.CrystalState.disable;
                    continue;
                }

                if (i >= Total - Used)
                {
                    GetItemAt(i).State = ViewModel.CrystalState.used;
                    continue;
                }

                if (i >= Total - Used - Highlight)
                {
                    GetItemAt(i).State = ViewModel.CrystalState.highlight;
                    continue;
                }

                GetItemAt(i).State = ViewModel.CrystalState.normal;
            }
        }
    }
}
