using Game.Animations;
using TouhouCardEngine;

namespace Game
{
    public interface IAnimationRef
    {
        DefineReference animRef { get; set; }
        ColorModifyData[] getColorModifies();
        ColorModifyData getColorModify(int index);
        void clearColorModify(DefineReference animRef);
        void addColorModify(ColorModifyData modify);
    }
}
