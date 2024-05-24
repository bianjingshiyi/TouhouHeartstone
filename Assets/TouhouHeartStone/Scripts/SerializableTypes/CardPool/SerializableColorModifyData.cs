using System;
using Game.Animations;
using MessagePack;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableColorModifyData
    {
        public SerializableColorModifyData(ColorModifyData colorTarget)
        {
            targetIndex = colorTarget.targetIndex;
            r = colorTarget.r;
            g = colorTarget.g;
            b = colorTarget.b;
            a = colorTarget.a;
        }
        public ColorModifyData deserialize()
        {
            return new ColorModifyData()
            {
                targetIndex = targetIndex,
                r = r,
                g = g,
                b = b,
                a = a
            };
        }
        [Key(0)]
        public int targetIndex;
        [Key(1)]
        public float r;
        [Key(2)]
        public float g;
        [Key(3)]
        public float b;
        [Key(4)]
        public float a;
    }
}
