
using Game;
using TouhouCardEngine;
using UI;
namespace Game.Animations 
{ 
    public static class SkinAnimationHelper
    {

        public static SkinAnimationRef getNodeAnimationRef(this Node node, ISkin skin)
        {
            var key = SkinAnimationRef.getNodeKey(node);
            if (string.IsNullOrEmpty(key))
                return null;
            return skin.getAnimRef(key);
        }
        public static string getOrInitNodeAnimationKey(this Node node, ISkin skin)
        {
            var key = SkinAnimationRef.getNodeKey(node);
            if (string.IsNullOrEmpty(key))
            {
                var animRefs = skin.getAllAnimRefs();
                key = $"AnimRef{animRefs.Length}_{node.id}";
                SkinAnimationRef.setNodeKey(node, key);
            }
            return key;
        }
        public static SkinAnimationRef getOrInitNodeAnimationRef(this Node node, ISkin skin)
        {
            var key = node.getOrInitNodeAnimationKey(skin);
            var animRef = skin.getAnimRef(key);
            if (animRef == null)
            {
                animRef = new SkinAnimationRef(key, new DefineReference(0, 0));
                skin.addAnimRef(animRef);
            }
            return animRef;
        }
    }
}
