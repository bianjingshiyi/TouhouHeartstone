using IGensoukyo.Utilities;
using System;
using System.Collections.Generic;
using TouhouHeartstone.Frontend.ViewModel;
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
                    GetItemAt(i).State = ViewModel.CrystalItemState.hidden;
                    continue;
                }

                if (Disable > i)
                {
                    GetItemAt(i).State = ViewModel.CrystalItemState.disable;
                    continue;
                }

                if (i >= Total - Used)
                {
                    GetItemAt(i).State = ViewModel.CrystalItemState.used;
                    continue;
                }

                if (i >= Total - Used - Highlight)
                {
                    GetItemAt(i).State = ViewModel.CrystalItemState.highlight;
                    continue;
                }

                GetItemAt(i).State = ViewModel.CrystalItemState.normal;
            }
        }
    }

}
