using UI;

namespace Game
{
    public interface ISkin
    {
        int skinId { get; set; }
        SkinAnimationRef[] getAllAnimRefs();
        SkinAnimationRef getAnimRef(string key);
        void addAnimRef(SkinAnimationRef animRef);
        int removeAnimRef(string key);
    }
}
