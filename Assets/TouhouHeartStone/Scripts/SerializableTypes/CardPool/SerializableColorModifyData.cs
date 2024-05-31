using System;
using Game.Animations;
using MessagePack;
using TouhouCardEngine;
using UnityEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableColorModifyData
    {
        public SerializableColorModifyData(ColorModifyData colorTarget)
        {
            animationRef = colorTarget.animationRef;
            targetIndex = colorTarget.targetIndex;
            r = colorTarget.r;
            g = colorTarget.g;
            b = colorTarget.b;
        }
        public ColorModifyData deserialize()
        {
            return new ColorModifyData()
            {
                animationRef = animationRef,
                targetIndex = targetIndex,
                r = r,
                g = g,
                b = b,
            };
        }
        [Key(0)]
        public DefineReference animationRef;
        [Key(1)]
        public int targetIndex;
        [Key(2)]
        public float r;
        [Key(3)]
        public float g;
        [Key(4)]
        public float b;
    }
}
