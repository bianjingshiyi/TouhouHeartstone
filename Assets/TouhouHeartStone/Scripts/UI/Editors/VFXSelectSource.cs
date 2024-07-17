using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    public abstract class VFXSelectSource
    {
        public VFXSelectSource()
        {
        }
        public abstract string[] getAnimationNames();
        public abstract Color getAnimationColor(int index);
        public abstract VFXPreviewData getPreviewData();
        public abstract VisualEffectDialogViewData getAnimationViewData();
        public abstract ColorModifyViewData[] getColorViewDatas();
        public abstract void setAnimationColor(int index, Color color);
        public abstract void changeProp(VisualEffectDialogInfo info);
        public abstract void resetColors();
    }
}
