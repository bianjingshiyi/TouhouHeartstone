using System.Linq;
using Game.Animations;

namespace Game
{
    public partial class AnimationManager
    {
        public AnimationDefine[] getAnimationDefinesForBuff()
        {
            return getAllAnimationDefines(true);
        }
        public string[] getAnimationEditorNamesForBuff()
        {
            return getAnimationDefinesForBuff().Select(d => main.lang._(d.name)).Prepend(main.lang._(StringTable.NONE)).ToArray();
        }
    }
}
